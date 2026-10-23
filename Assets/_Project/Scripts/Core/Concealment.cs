using UnityEngine;

namespace DarkDepths.Core
{
    // The one rule of cover from docs/05, kept pure: a hiding place hides until someone who is
    // searching comes right up to it. Anyone not searching walks past whatever is inside.
    public static class Concealment
    {
        public static bool Hides(Vector3 place, Vector3 from, float revealDistance, bool searching)
        {
            if (!searching || revealDistance <= 0f)
            {
                return true;
            }

            // Closer than the distance, not at it: the edge of the reach is still outside it.
            return (from - place).sqrMagnitude >= revealDistance * revealDistance;
        }
    }
}
