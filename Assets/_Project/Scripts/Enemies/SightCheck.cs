using UnityEngine;

namespace DarkDepths.Enemies
{
    // The cone of docs/05, without the raycast: how far this enemy sees and whether a point is inside
    // the wedge it is looking down. Kept pure so the numbers can be checked without a scene.
    public static class SightCheck
    {
        // A lit player is visible from further away; the beam is what gives them away.
        public static float RangeFor(float sightRange, float lightSensitivity, bool lit)
        {
            if (sightRange <= 0f)
            {
                return 0f;
            }

            return lit ? sightRange * (1f + Mathf.Max(0f, lightSensitivity)) : sightRange;
        }

        // Range and angle only; whether a wall stands in the way is for physics to answer.
        public static bool IsInView(Vector3 eye, Vector3 forward, Vector3 target, float range, float angle)
        {
            if (range <= 0f)
            {
                return false;
            }

            Vector3 delta = target - eye;
            if (delta.sqrMagnitude > range * range)
            {
                return false;
            }

            Vector3 flatForward = forward;
            flatForward.y = 0f;
            Vector3 flatDelta = delta;
            flatDelta.y = 0f;

            // Standing on top of the enemy, or an enemy with no heading: there is nothing to be outside of.
            if (flatDelta.sqrMagnitude < 0.0001f || flatForward.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            return Vector3.Angle(flatForward, flatDelta) <= angle * 0.5f;
        }
    }
}
