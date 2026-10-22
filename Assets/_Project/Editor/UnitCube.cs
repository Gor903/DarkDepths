using UnityEngine;

namespace DarkDepths.EditorTools
{
    // The cube every generated mesh is built from, cached so a tool does not spawn one primitive per box.
    public static class UnitCube
    {
        private static Mesh _mesh;

        public static Mesh Mesh
        {
            get
            {
                if (_mesh != null)
                {
                    return _mesh;
                }

                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(primitive);
                return _mesh;
            }
        }
    }
}
