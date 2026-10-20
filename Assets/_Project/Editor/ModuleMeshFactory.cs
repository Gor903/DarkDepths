using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Collects the boxes a module is made of and bakes them into one mesh asset, so a module is one draw call.
    public sealed class ModuleMeshFactory
    {
        private const string MeshFolder = "Assets/_Project/Art/Models/Generated";

        private static Mesh _unitCube;

        private readonly List<Bounds> _boxes = new List<Bounds>();

        public IReadOnlyList<Bounds> Boxes => _boxes;

        public ModuleMeshFactory Add(Vector3 center, Vector3 size)
        {
            _boxes.Add(new Bounds(center, size));
            return this;
        }

        public Mesh Save(string meshName)
        {
            var instances = new CombineInstance[_boxes.Count];
            for (int i = 0; i < _boxes.Count; i++)
            {
                instances[i] = new CombineInstance
                {
                    mesh = UnitCube(),
                    transform = Matrix4x4.TRS(_boxes[i].center, Quaternion.identity, _boxes[i].size)
                };
            }

            var mesh = new Mesh { name = meshName };
            mesh.CombineMeshes(instances, true, true);
            mesh.RecalculateBounds();
            // Lightmap UVs now, because docs/02 bakes the lamps and this geometry is what they light.
            Unwrapping.GenerateSecondaryUVSet(mesh);

            Directory.CreateDirectory(MeshFolder);
            string path = MeshFolder + "/" + meshName + ".asset";

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            // Re-filling the existing asset keeps every prefab and scene pointing at the same mesh.
            existing.Clear();
            existing.CombineMeshes(instances, true, true);
            existing.RecalculateBounds();
            Unwrapping.GenerateSecondaryUVSet(existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Mesh UnitCube()
        {
            if (_unitCube != null)
            {
                return _unitCube;
            }

            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _unitCube = primitive.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(primitive);
            return _unitCube;
        }
    }
}
