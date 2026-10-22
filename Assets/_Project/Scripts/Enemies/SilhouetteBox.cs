using UnityEngine;

namespace DarkDepths.Enemies
{
    // One box of the body, measured in the local space of the joint that carries it.
    public readonly struct SilhouetteBox
    {
        public SilhouetteBox(SilhouetteJoint joint, Vector3 center, Vector3 size)
        {
            Joint = joint;
            Center = center;
            Size = size;
        }

        public SilhouetteJoint Joint { get; }
        public Vector3 Center { get; }
        public Vector3 Size { get; }

        public Vector3 Corner(int index)
        {
            return new Vector3(
                Center.x + ((index & 1) == 0 ? -0.5f : 0.5f) * Size.x,
                Center.y + ((index & 2) == 0 ? -0.5f : 0.5f) * Size.y,
                Center.z + ((index & 4) == 0 ? -0.5f : 0.5f) * Size.z);
        }
    }
}
