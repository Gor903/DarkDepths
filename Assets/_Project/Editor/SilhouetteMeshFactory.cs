using System.IO;
using DarkDepths.Enemies;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Bakes a body into one skinned mesh: each box rides a single bone, so an enemy costs one draw call.
    public static class SilhouetteMeshFactory
    {
        private const string MeshFolder = "Assets/_Project/Art/Models/Generated";

        public static Mesh Bake(SilhouetteRig rig, string meshName)
        {
            // One instance per box, in rig order: that order is what the bone weights below are assigned along.
            var instances = new CombineInstance[rig.Boxes.Count];
            for (int i = 0; i < instances.Length; i++)
            {
                SilhouetteBox box = rig.Boxes[i];
                instances[i] = new CombineInstance
                {
                    mesh = UnitCube.Mesh,
                    // A box is measured on its joint and baked where that joint rests.
                    transform = rig.BoneToRoot(box.Joint) * Matrix4x4.TRS(box.Center, Quaternion.identity, box.Size)
                };
            }

            var mesh = new Mesh { name = meshName };
            mesh.CombineMeshes(instances, true, true);
            Skin(mesh, rig, instances);
            mesh.RecalculateBounds();

            return Store(mesh, meshName);
        }

        private static void Skin(Mesh mesh, SilhouetteRig rig, CombineInstance[] instances)
        {
            // One bone per vertex: a box figure never bends, it only swings whole limbs.
            var weights = new BoneWeight[mesh.vertexCount];
            int vertex = 0;

            for (int i = 0; i < instances.Length; i++)
            {
                var weight = new BoneWeight { boneIndex0 = (int)rig.Boxes[i].Joint, weight0 = 1f };
                int count = instances[i].mesh.vertexCount;

                for (int k = 0; k < count && vertex < weights.Length; k++)
                {
                    weights[vertex++] = weight;
                }
            }

            var bindPoses = new Matrix4x4[SilhouetteRig.JointCount];
            for (int i = 0; i < bindPoses.Length; i++)
            {
                bindPoses[i] = rig.BoneToRoot((SilhouetteJoint)i).inverse;
            }

            mesh.boneWeights = weights;
            mesh.bindposes = bindPoses;
        }

        private static Mesh Store(Mesh mesh, string meshName)
        {
            Directory.CreateDirectory(MeshFolder);
            string path = MeshFolder + "/" + meshName + ".asset";

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            // Re-filling the existing asset keeps the prefab pointing at the same mesh.
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }
    }
}
