using System.IO;
using DarkDepths.Enemies;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Bakes the three clips docs/02 asks a silhouette for: stillness, a wrong walk, a sharp head turn.
    public static class SilhouetteClipFactory
    {
        private const string ClipFolder = "Assets/_Project/Art/Animation";

        private const string RotationX = "localEulerAnglesRaw.x";
        private const string RotationY = "localEulerAnglesRaw.y";
        private const string RotationZ = "localEulerAnglesRaw.z";
        private const string PositionX = "m_LocalPosition.x";
        private const string PositionY = "m_LocalPosition.y";
        private const string PositionZ = "m_LocalPosition.z";

        private const float Tau = Mathf.PI * 2f;

        // Breathing and a head that drifts, nothing else: what stands still is what the sudden move reads against.
        public static AnimationClip Idle(string clipName, SilhouetteConfig config, SilhouetteRig rig)
        {
            int samples = config.CycleSamples;
            var times = new float[samples + 1];
            var breath = new float[samples + 1];
            var drift = new float[samples + 1];

            Vector3 chestRest = rig.LocalAngles(SilhouetteJoint.Chest);
            Vector3 headRest = rig.LocalAngles(SilhouetteJoint.Head);

            for (int i = 0; i <= samples; i++)
            {
                float phase = (float)i / samples;
                times[i] = config.IdleDuration * phase;
                breath[i] = chestRest.x + config.IdleSwayAngle * Mathf.Sin(Tau * phase);
                // A quarter cycle off the breath, so the two never line up into one bob.
                drift[i] = headRest.y + config.IdleHeadDriftAngle * Mathf.Cos(Tau * phase);
            }

            AnimationClip clip = NewClip(config);
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Chest), times, chestRest, breath, null, null, true);
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Head), times, headRest, null, drift, null, true);

            return Store(clip, clipName, true);
        }

        public static AnimationClip Walk(string clipName, SilhouetteConfig config, SilhouetteRig rig)
        {
            SilhouetteGait gait = config.CreateGait();

            int samples = config.CycleSamples;
            var times = new float[samples + 1];
            var bob = new float[samples + 1];
            var sway = new float[samples + 1];
            var scan = new float[samples + 1];

            Vector3 hipsRest = rig.LocalPosition(SilhouetteJoint.Hips);
            Vector3 chestRest = rig.LocalAngles(SilhouetteJoint.Chest);
            Vector3 headRest = rig.LocalAngles(SilhouetteJoint.Head);

            for (int i = 0; i <= samples; i++)
            {
                float phase = (float)i / samples;
                times[i] = config.StrideDuration * phase;
                bob[i] = hipsRest.y + gait.Bob(phase);
                sway[i] = chestRest.z + gait.TorsoSway(phase);
                scan[i] = headRest.y + gait.HeadScan(phase);
            }

            AnimationClip clip = NewClip(config);
            SetPosition(clip, rig.PathOf(SilhouetteJoint.Hips), times, hipsRest, bob);
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Chest), times, chestRest, null, null, sway, true);
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Head), times, headRest, null, scan, null, true);

            for (int i = 0; i < 2; i++)
            {
                bool right = i == 1;
                AddLeg(clip, rig, gait, times, right);
                AddArm(clip, rig, gait, times, right);
            }

            return Store(clip, clipName, true);
        }

        // Snaps round, holds, then drifts back; the chest follows a little or the neck reads as broken.
        public static AnimationClip HeadTurn(string clipName, SilhouetteConfig config, SilhouetteRig rig)
        {
            float turned = config.HeadTurnTime;
            float held = turned + config.HeadTurnHold;
            float back = held + config.HeadTurnReturnTime;
            var times = new[] { 0f, turned, held, back };

            Vector3 headRest = rig.LocalAngles(SilhouetteJoint.Head);
            Vector3 chestRest = rig.LocalAngles(SilhouetteJoint.Chest);

            float turnedTo = headRest.y + config.HeadTurnAngle;
            var head = new[] { headRest.y, turnedTo, turnedTo, headRest.y };

            float followedTo = chestRest.y + config.HeadTurnChestAngle;
            var chest = new[] { chestRest.y, followedTo, followedTo, chestRest.y };

            AnimationClip clip = NewClip(config);
            // Linear keys: eased ones would turn the snap into a look.
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Head), times, headRest, null, head, null, false);
            SetRotation(clip, rig.PathOf(SilhouetteJoint.Chest), times, chestRest, null, chest, null, false);

            return Store(clip, clipName, false);
        }

        private static void AddLeg(AnimationClip clip, SilhouetteRig rig, SilhouetteGait gait, float[] times, bool right)
        {
            SilhouetteJoint thigh = right ? SilhouetteJoint.ThighRight : SilhouetteJoint.ThighLeft;
            SilhouetteJoint shin = right ? SilhouetteJoint.ShinRight : SilhouetteJoint.ShinLeft;

            Vector3 thighRest = rig.LocalAngles(thigh);
            Vector3 shinRest = rig.LocalAngles(shin);

            var thighAngles = new float[times.Length];
            var shinAngles = new float[times.Length];
            int samples = times.Length - 1;

            for (int i = 0; i < times.Length; i++)
            {
                float phase = (float)i / samples;
                // Limbs hang down, so swinging one forward turns its joint the other way about x.
                thighAngles[i] = thighRest.x - gait.ThighAngle(phase, right);
                shinAngles[i] = shinRest.x - gait.ShinAngle(phase, right);
            }

            SetRotation(clip, rig.PathOf(thigh), times, thighRest, thighAngles, null, null, true);
            SetRotation(clip, rig.PathOf(shin), times, shinRest, shinAngles, null, null, true);
        }

        private static void AddArm(AnimationClip clip, SilhouetteRig rig, SilhouetteGait gait, float[] times, bool right)
        {
            SilhouetteJoint arm = right ? SilhouetteJoint.ArmRight : SilhouetteJoint.ArmLeft;

            Vector3 armRest = rig.LocalAngles(arm);
            var armAngles = new float[times.Length];
            int samples = times.Length - 1;

            for (int i = 0; i < times.Length; i++)
            {
                float phase = (float)i / samples;
                armAngles[i] = armRest.x - gait.ArmAngle(phase, right);
            }

            SetRotation(clip, rig.PathOf(arm), times, armRest, armAngles, null, null, true);
        }

        // Animating one euler axis rebuilds the whole rotation, so the two that rest have to be written as well.
        private static void SetRotation(AnimationClip clip, string path, float[] times, Vector3 rest,
            float[] x, float[] y, float[] z, bool smooth)
        {
            SetCurve(clip, path, RotationX, times, x ?? Held(times.Length, rest.x), smooth);
            SetCurve(clip, path, RotationY, times, y ?? Held(times.Length, rest.y), smooth);
            SetCurve(clip, path, RotationZ, times, z ?? Held(times.Length, rest.z), smooth);
        }

        // Only the bob moves, but a partial position behaves like a partial rotation, so all three go in.
        private static void SetPosition(AnimationClip clip, string path, float[] times, Vector3 rest, float[] y)
        {
            SetCurve(clip, path, PositionX, times, Held(times.Length, rest.x), true);
            SetCurve(clip, path, PositionY, times, y, true);
            SetCurve(clip, path, PositionZ, times, Held(times.Length, rest.z), true);
        }

        private static float[] Held(int count, float value)
        {
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = value;
            }

            return values;
        }

        private static AnimationClip NewClip(SilhouetteConfig config)
        {
            return new AnimationClip { frameRate = config.ClipFrameRate };
        }

        private static void SetCurve(AnimationClip clip, string path, string property, float[] times, float[] values,
            bool smooth)
        {
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
            {
                keys[i] = new Keyframe(times[i], values[i]);
            }

            var curve = new AnimationCurve(keys);
            AnimationUtility.TangentMode mode = smooth
                ? AnimationUtility.TangentMode.ClampedAuto
                : AnimationUtility.TangentMode.Linear;

            // Default keys come out flat, which would make a sampled curve step from key to key.
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
                AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
            }

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
        }

        private static AnimationClip Store(AnimationClip clip, string clipName, bool loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            Directory.CreateDirectory(ClipFolder);
            string path = ClipFolder + "/" + clipName + ".anim";

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(clip, path);
                return clip;
            }

            // Re-filling the existing clip keeps the controller pointing at the same asset.
            EditorUtility.CopySerialized(clip, existing);
            Object.DestroyImmediate(clip);
            EditorUtility.SetDirty(existing);
            return existing;
        }
    }
}
