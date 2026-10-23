using UnityEngine;

namespace DarkDepths.Enemies
{
    // Everything the brain is allowed to do to the world, and everything it is allowed to know about it.
    // Behind it sits a NavMeshAgent in the game and a stand-in in the tests, so the states can be
    // checked without a scene.
    public interface IEnemyBody
    {
        Vector3 Position { get; }

        bool HasArrived { get; }

        // The player, as far as this enemy can tell right now.
        bool CanSeeTarget { get; }

        bool HasTarget { get; }

        Vector3 TargetPosition { get; }

        float DistanceToTarget { get; }

        void MoveTo(Vector3 destination, float speed);

        void Stop();

        // The visible reaction of docs/05: the head turns before the body does.
        void LookAround();

        // A point on walkable ground near the centre; false when there is none to be found.
        bool TrySamplePointNear(Vector3 center, float radius, out Vector3 point);

        void Strike();

        // While an enemy is chasing, the player is told; the screen shows it (docs/04).
        void BeginHunt();

        void EndHunt();
    }
}
