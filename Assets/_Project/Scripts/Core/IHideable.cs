using UnityEngine;

namespace DarkDepths.Core
{
    // Whoever can take cover. A hiding place talks to this and never to the player directly.
    public interface IHideable
    {
        bool IsHidden { get; }

        void Hide(Transform anchor, Vector3 place, float revealDistance);

        void Leave();
    }
}
