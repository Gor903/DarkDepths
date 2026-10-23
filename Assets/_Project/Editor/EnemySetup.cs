using System.IO;
using DarkDepths.Core;
using DarkDepths.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

namespace DarkDepths.EditorTools
{
    // Builds the silhouette enemy of docs/02: one skinned box figure in the palette's enemy colour, three clips.
    public static class EnemySetup
    {
        public const string Listener = "Enemy_Listener";

        private const string ConfigFolder = "Assets/_Project/Config";
        private const string ConfigPath = ConfigFolder + "/SilhouetteConfig.asset";
        private const string DefinitionPath = ConfigFolder + "/ListenerDefinition.asset";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Enemies";
        private const string AnimationFolder = "Assets/_Project/Art/Animation";
        private const string MeshName = "Silhouette_Listener";
        private const string SkinName = "Silhouette";

        private const string WalkingParameter = "Walking";
        private const string HeadTurnParameter = "HeadTurn";
        private const string IdleState = "Idle";
        private const string WalkState = "Walk";
        private const string HeadTurnState = "HeadTurn";

        [MenuItem("DarkDepths/Setup/Create Enemies")]
        public static void CreateEnemies()
        {
            LayersSetup.CreateLayers();

            SilhouetteConfig config = LoadOrCreateConfig();
            PaletteConfig palette = PaletteSetup.LoadOrCreateConfig();
            Material material = PaletteSetup.MaterialFor(PaletteRole.Enemy, palette);

            var rig = new SilhouetteRig(config.CreateProportions());

            EnemyDefinition definition = LoadOrCreateDefinition();

            Directory.CreateDirectory(PrefabFolder);
            GameObject root = BuildListener(config, rig, material, definition);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + Listener + ".prefab");
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
        }

        public static SilhouetteConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SilhouetteConfig>(ConfigPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(ConfigFolder);
            var created = ScriptableObject.CreateInstance<SilhouetteConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            return created;
        }

        public static EnemyDefinition LoadOrCreateDefinition()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(DefinitionPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(ConfigFolder);
            // The field defaults are the Listener's starting values from docs/05.
            var created = ScriptableObject.CreateInstance<EnemyDefinition>();
            AssetDatabase.CreateAsset(created, DefinitionPath);
            return created;
        }

        public static GameObject LoadListener()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + Listener + ".prefab");
        }

        private static GameObject BuildListener(SilhouetteConfig config, SilhouetteRig rig, Material material,
            EnemyDefinition definition)
        {
            var root = new GameObject(Listener, typeof(Animator), typeof(SilhouetteAnimator));
            root.layer = LayerMask.NameToLayer("Enemy");

            Transform[] bones = BuildBones(root.transform, rig);
            BuildSkin(root.transform, rig, bones, material);

            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = BuildController(config, rig);
            animator.applyRootMotion = false;
            // Off-screen enemies keep their state but stop posing bones; docs/07 budgets four of them at once.
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            SerializedWiring.Wire(root.GetComponent<SilhouetteAnimator>(), "_animator", animator);

            BuildNavigation(root, rig);
            var brain = root.AddComponent<EnemyAgent>();
            SerializedWiring.Wire(brain, "_definition", definition);
            SerializedWiring.Wire(brain, "_animator", root.GetComponent<SilhouetteAnimator>());

            return root;
        }

        // Slimmer than the baked agent radius: the silhouette has to fit a two-metre corridor beside a wall.
        private static void BuildNavigation(GameObject root, SilhouetteRig rig)
        {
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.3f;
            agent.height = rig.Height;
            agent.baseOffset = 0f;
            agent.angularSpeed = 240f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
        }

        private static Transform[] BuildBones(Transform root, SilhouetteRig rig)
        {
            var bones = new Transform[SilhouetteRig.JointCount];

            for (int i = 0; i < bones.Length; i++)
            {
                var joint = (SilhouetteJoint)i;
                var bone = new GameObject(SilhouetteRig.NameOf(joint));

                int parent = SilhouetteRig.ParentOf(joint);
                bone.transform.SetParent(parent < 0 ? root : bones[parent], false);
                bone.transform.localPosition = rig.LocalPosition(joint);

                Vector3 angles = rig.LocalAngles(joint);
                bone.transform.localRotation = Quaternion.Euler(angles);
                // Without the hint the inspector rounds a resting joint to some other pair of equivalent angles.
                TransformUtils.SetInspectorRotation(bone.transform, angles);

                bones[i] = bone.transform;
            }

            return bones;
        }

        private static void BuildSkin(Transform root, SilhouetteRig rig, Transform[] bones, Material material)
        {
            var skin = new GameObject(SkinName, typeof(SkinnedMeshRenderer));
            skin.transform.SetParent(root, false);
            skin.layer = root.gameObject.layer;

            var renderer = skin.GetComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = SilhouetteMeshFactory.Bake(rig, MeshName);
            renderer.sharedMaterial = material;
            renderer.bones = bones;
            renderer.rootBone = bones[(int)SilhouetteJoint.Hips];
            // One bone per vertex is all a box figure needs, and the cheapest skinning on a phone.
            renderer.quality = SkinQuality.Bone1;
            renderer.updateWhenOffscreen = false;
            renderer.localBounds = rig.Bounds;
        }

        private static AnimatorController BuildController(SilhouetteConfig config, SilhouetteRig rig)
        {
            Directory.CreateDirectory(AnimationFolder);
            string path = AnimationFolder + "/" + Listener + ".controller";

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            Clear(controller);
            controller.AddParameter(WalkingParameter, AnimatorControllerParameterType.Bool);
            controller.AddParameter(HeadTurnParameter, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = machine.AddState(IdleState);
            idle.motion = SilhouetteClipFactory.Idle(Listener + "_" + IdleState, config, rig);
            AnimatorState walk = machine.AddState(WalkState);
            walk.motion = SilhouetteClipFactory.Walk(Listener + "_" + WalkState, config, rig);
            AnimatorState turn = machine.AddState(HeadTurnState);
            turn.motion = SilhouetteClipFactory.HeadTurn(Listener + "_" + HeadTurnState, config, rig);

            machine.defaultState = idle;

            Transition(idle, walk, config).AddCondition(AnimatorConditionMode.If, 0f, WalkingParameter);
            Transition(walk, idle, config).AddCondition(AnimatorConditionMode.IfNot, 0f, WalkingParameter);
            Transition(idle, turn, config).AddCondition(AnimatorConditionMode.If, 0f, HeadTurnParameter);

            // The turn plays out before anything else happens; it is a reaction, not a pose to hold.
            AnimatorStateTransition done = Transition(turn, idle, config);
            done.hasExitTime = true;
            done.exitTime = 1f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, SilhouetteConfig config)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = config.BlendTime;
            return transition;
        }

        // Re-running rebuilds the states in the same asset, so the prefab keeps its controller.
        private static void Clear(AnimatorController controller)
        {
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                machine.RemoveState(states[i].state);
            }

            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                controller.RemoveParameter(parameters[i]);
            }
        }
    }
}
