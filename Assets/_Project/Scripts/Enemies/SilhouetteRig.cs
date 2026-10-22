using System.Collections.Generic;
using UnityEngine;

namespace DarkDepths.Enemies
{
    // Lays a body out from its measurements: where each joint sits, how it rests and which box it carries.
    public sealed class SilhouetteRig
    {
        public const int JointCount = 13;

        private const int CornerCount = 8;
        // The hunch is split so the body bends over two joints instead of snapping at one.
        private const float SpineHunchShare = 0.6f;
        // Neck and head undo the hunch between them, which is what keeps the face aimed forward.
        private const float NeckLiftShare = 0.7f;

        private static readonly string[] Names =
        {
            "Hips", "Spine", "Chest", "Neck", "Head",
            "ArmLeft", "ForearmLeft", "ArmRight", "ForearmRight",
            "ThighLeft", "ShinLeft", "ThighRight", "ShinRight"
        };

        private static readonly int[] Parents =
        {
            -1,
            (int)SilhouetteJoint.Hips,
            (int)SilhouetteJoint.Spine,
            (int)SilhouetteJoint.Chest,
            (int)SilhouetteJoint.Neck,
            (int)SilhouetteJoint.Chest,
            (int)SilhouetteJoint.ArmLeft,
            (int)SilhouetteJoint.Chest,
            (int)SilhouetteJoint.ArmRight,
            (int)SilhouetteJoint.Hips,
            (int)SilhouetteJoint.ThighLeft,
            (int)SilhouetteJoint.Hips,
            (int)SilhouetteJoint.ThighRight
        };

        private readonly Vector3[] _localPositions = new Vector3[JointCount];
        private readonly Vector3[] _localAngles = new Vector3[JointCount];
        private readonly Matrix4x4[] _boneToRoot = new Matrix4x4[JointCount];
        private readonly string[] _paths = new string[JointCount];
        private readonly SilhouetteBox[] _boxes;

        public SilhouetteRig(SilhouetteProportions body)
        {
            PlaceJoints(body);

            for (int i = 0; i < JointCount; i++)
            {
                Matrix4x4 local = Matrix4x4.TRS(_localPositions[i], Quaternion.Euler(_localAngles[i]), Vector3.one);
                _boneToRoot[i] = Parents[i] < 0 ? local : _boneToRoot[Parents[i]] * local;
                _paths[i] = Parents[i] < 0 ? Names[i] : _paths[Parents[i]] + "/" + Names[i];
            }

            _boxes = BuildBoxes(body);
            Bounds = Measure();
        }

        // What the body occupies in root space, which is also what the renderer is culled by.
        public Bounds Bounds { get; }

        public float Height => Bounds.max.y;

        public IReadOnlyList<SilhouetteBox> Boxes => _boxes;

        public static string NameOf(SilhouetteJoint joint)
        {
            return Names[(int)joint];
        }

        // The joint this one hangs off, or -1 for the hips.
        public static int ParentOf(SilhouetteJoint joint)
        {
            return Parents[(int)joint];
        }

        public Vector3 LocalPosition(SilhouetteJoint joint)
        {
            return _localPositions[(int)joint];
        }

        public Vector3 LocalAngles(SilhouetteJoint joint)
        {
            return _localAngles[(int)joint];
        }

        public Matrix4x4 BoneToRoot(SilhouetteJoint joint)
        {
            return _boneToRoot[(int)joint];
        }

        // Transform path from the body root; animation curves are bound to it.
        public string PathOf(SilhouetteJoint joint)
        {
            return _paths[(int)joint];
        }

        private void PlaceJoints(SilhouetteProportions body)
        {
            float hunch = body.HunchAngle;

            // The hips ride on top of the legs, so the feet land on y = 0 and the body needs no offset.
            Set(SilhouetteJoint.Hips, new Vector3(0f, body.ThighLength + body.ShinLength, 0f), Vector3.zero);
            Set(SilhouetteJoint.Spine, new Vector3(0f, body.PelvisHeight, 0f),
                new Vector3(hunch * SpineHunchShare, 0f, 0f));
            Set(SilhouetteJoint.Chest, new Vector3(0f, body.SpineLength, 0f),
                new Vector3(hunch * (1f - SpineHunchShare), 0f, 0f));
            Set(SilhouetteJoint.Neck, new Vector3(0f, body.ChestHeight, 0f),
                new Vector3(-hunch * NeckLiftShare, 0f, 0f));
            // The cocked head is the one thing that reads as listening even when the body is still.
            Set(SilhouetteJoint.Head, new Vector3(0f, body.NeckLength, 0f),
                new Vector3(-hunch * (1f - NeckLiftShare), 0f, body.HeadCockAngle));

            float shoulderX = (body.ChestWidth + body.ArmThickness) * 0.5f;
            float shoulderY = body.ChestHeight - body.ShoulderDrop;
            float hipX = (body.WaistWidth - body.LegThickness) * 0.5f;

            for (int i = 0; i < 2; i++)
            {
                bool right = i == 1;
                float sign = right ? 1f : -1f;

                Set(right ? SilhouetteJoint.ArmRight : SilhouetteJoint.ArmLeft,
                    new Vector3(sign * shoulderX, shoulderY, 0f),
                    new Vector3(body.ArmRestAngle, 0f, sign * body.ArmSpreadAngle));
                Set(right ? SilhouetteJoint.ForearmRight : SilhouetteJoint.ForearmLeft,
                    new Vector3(0f, -body.UpperArmLength, 0f),
                    new Vector3(body.ElbowRestAngle, 0f, 0f));
                Set(right ? SilhouetteJoint.ThighRight : SilhouetteJoint.ThighLeft,
                    new Vector3(sign * hipX, 0f, 0f), Vector3.zero);
                Set(right ? SilhouetteJoint.ShinRight : SilhouetteJoint.ShinLeft,
                    new Vector3(0f, -body.ThighLength, 0f), Vector3.zero);
            }
        }

        private void Set(SilhouetteJoint joint, Vector3 position, Vector3 angles)
        {
            _localPositions[(int)joint] = position;
            _localAngles[(int)joint] = angles;
        }

        private static SilhouetteBox[] BuildBoxes(SilhouetteProportions body)
        {
            // The spine tapers from waist to chest, so the torso reads as a shape rather than a slab.
            float spineWidth = (body.WaistWidth + body.ChestWidth) * 0.5f;
            float spineDepth = (body.WaistDepth + body.ChestDepth) * 0.5f;

            var boxes = new SilhouetteBox[JointCount];
            boxes[(int)SilhouetteJoint.Hips] = Upright(SilhouetteJoint.Hips,
                body.WaistWidth, body.PelvisHeight, body.WaistDepth);
            boxes[(int)SilhouetteJoint.Spine] = Upright(SilhouetteJoint.Spine,
                spineWidth, body.SpineLength, spineDepth);
            boxes[(int)SilhouetteJoint.Chest] = Upright(SilhouetteJoint.Chest,
                body.ChestWidth, body.ChestHeight, body.ChestDepth);
            boxes[(int)SilhouetteJoint.Neck] = Upright(SilhouetteJoint.Neck,
                body.ArmThickness, body.NeckLength, body.ArmThickness);
            boxes[(int)SilhouetteJoint.Head] = Upright(SilhouetteJoint.Head,
                body.HeadWidth, body.HeadHeight, body.HeadDepth);

            for (int i = 0; i < 2; i++)
            {
                bool right = i == 1;

                boxes[(int)(right ? SilhouetteJoint.ArmRight : SilhouetteJoint.ArmLeft)] =
                    Hanging(right ? SilhouetteJoint.ArmRight : SilhouetteJoint.ArmLeft,
                        body.ArmThickness, body.UpperArmLength);
                boxes[(int)(right ? SilhouetteJoint.ForearmRight : SilhouetteJoint.ForearmLeft)] =
                    Hanging(right ? SilhouetteJoint.ForearmRight : SilhouetteJoint.ForearmLeft,
                        body.ArmThickness, body.ForearmLength);
                boxes[(int)(right ? SilhouetteJoint.ThighRight : SilhouetteJoint.ThighLeft)] =
                    Hanging(right ? SilhouetteJoint.ThighRight : SilhouetteJoint.ThighLeft,
                        body.LegThickness, body.ThighLength);
                boxes[(int)(right ? SilhouetteJoint.ShinRight : SilhouetteJoint.ShinLeft)] =
                    Hanging(right ? SilhouetteJoint.ShinRight : SilhouetteJoint.ShinLeft,
                        body.LegThickness, body.ShinLength);
            }

            return boxes;
        }

        // Torso pieces stack upwards from their joint.
        private static SilhouetteBox Upright(SilhouetteJoint joint, float width, float height, float depth)
        {
            return new SilhouetteBox(joint, new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth));
        }

        // Limbs hang down from their joint, so rotating the joint swings the whole segment.
        private static SilhouetteBox Hanging(SilhouetteJoint joint, float thickness, float length)
        {
            return new SilhouetteBox(joint, new Vector3(0f, -length * 0.5f, 0f),
                new Vector3(thickness, length, thickness));
        }

        private Bounds Measure()
        {
            var bounds = new Bounds(_boneToRoot[0].MultiplyPoint3x4(_boxes[0].Center), Vector3.zero);

            for (int i = 0; i < _boxes.Length; i++)
            {
                Matrix4x4 boneToRoot = _boneToRoot[(int)_boxes[i].Joint];
                for (int corner = 0; corner < CornerCount; corner++)
                {
                    bounds.Encapsulate(boneToRoot.MultiplyPoint3x4(_boxes[i].Corner(corner)));
                }
            }

            return bounds;
        }
    }
}
