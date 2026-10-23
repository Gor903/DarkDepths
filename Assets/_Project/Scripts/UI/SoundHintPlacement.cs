using UnityEngine;

namespace DarkDepths.UI
{
    // Where on the ring around the screen a sound belongs, and how strongly.
    // Kept free of MonoBehaviour so the geometry can be covered by EditMode tests.
    public static class SoundHintPlacement
    {
        // Bearing is degrees clockwise from where the player is looking; strength falls off to nothing at range.
        public static bool TryPlace(Vector3 listener, Vector3 forward, Vector3 source, float maxDistance,
            out float bearing, out float strength)
        {
            bearing = 0f;
            strength = 0f;

            if (maxDistance <= 0f)
            {
                return false;
            }

            // Height is dropped: the hint is a direction to turn towards, and the player turns on one axis.
            Vector3 delta = source - listener;
            delta.y = 0f;

            float distance = delta.magnitude;
            if (distance >= maxDistance)
            {
                return false;
            }

            strength = 1f - distance / maxDistance;

            Vector3 flatForward = forward;
            flatForward.y = 0f;
            // A sound on top of the player, or a camera looking straight down, has no direction to point at.
            if (distance < 0.0001f || flatForward.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            bearing = Vector3.SignedAngle(flatForward.normalized, delta / distance, Vector3.up);
            return true;
        }
    }
}
