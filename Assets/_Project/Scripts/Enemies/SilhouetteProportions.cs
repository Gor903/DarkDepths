namespace DarkDepths.Enemies
{
    // Every measurement a silhouette is cut from: lengths in metres, posture in degrees.
    public sealed class SilhouetteProportions
    {
        public float ThighLength { get; set; }
        public float ShinLength { get; set; }
        public float LegThickness { get; set; }

        public float PelvisHeight { get; set; }
        public float WaistWidth { get; set; }
        public float WaistDepth { get; set; }
        public float SpineLength { get; set; }
        public float ChestHeight { get; set; }
        public float ChestWidth { get; set; }
        public float ChestDepth { get; set; }

        public float NeckLength { get; set; }
        public float HeadWidth { get; set; }
        public float HeadHeight { get; set; }
        public float HeadDepth { get; set; }

        public float UpperArmLength { get; set; }
        public float ForearmLength { get; set; }
        public float ArmThickness { get; set; }
        public float ShoulderDrop { get; set; }

        public float HunchAngle { get; set; }
        public float HeadCockAngle { get; set; }
        public float ArmRestAngle { get; set; }
        public float ElbowRestAngle { get; set; }
        public float ArmSpreadAngle { get; set; }
    }
}
