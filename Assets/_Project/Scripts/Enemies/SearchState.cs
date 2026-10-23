using UnityEngine;

namespace DarkDepths.Enemies
{
    // Casting around the last place something was heard. Each point reached is followed by another one,
    // until the search runs out of time and the enemy goes back to its route.
    public sealed class SearchState : IEnemyState
    {
        private Vector3 _center;

        public EnemyStateId Id => EnemyStateId.Search;

        public void Enter(EnemyBrain brain)
        {
            _center = brain.Interest;
            // What brought it here is dealt with; anything new from now on is worth hearing again.
            brain.ForgetInterest();
            PickPoint(brain);
        }

        public void Tick(EnemyBrain brain, float deltaTime)
        {
            if (brain.HasInterest)
            {
                brain.Switch(EnemyStateId.Investigate);
                return;
            }

            if (brain.TimeInState >= brain.Definition.SearchTime)
            {
                brain.Switch(EnemyStateId.Return);
                return;
            }

            if (brain.Body.HasArrived)
            {
                brain.Body.LookAround();
                PickPoint(brain);
            }
        }

        public void Exit(EnemyBrain brain)
        {
        }

        private void PickPoint(EnemyBrain brain)
        {
            if (brain.Body.TrySamplePointNear(_center, brain.Definition.SearchRadius, out Vector3 point))
            {
                brain.Body.MoveTo(point, brain.Definition.WalkSpeed);
                return;
            }

            // Nowhere to walk to: the enemy stands and listens rather than giving up on the spot.
            brain.Body.Stop();
        }
    }
}
