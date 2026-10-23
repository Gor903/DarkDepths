using System.Collections.Generic;
using UnityEngine;

namespace DarkDepths.Enemies
{
    // The state machine of docs/05 and the little the enemy remembers: where it patrols, where it came
    // from, and the loudest thing it has heard and not yet dealt with. No MonoBehaviour, no NavMesh:
    // the transitions can be read and tested on their own.
    public sealed class EnemyBrain
    {
        private readonly IEnemyState[] _states =
        {
            new PatrolState(),
            new AlertedState(),
            new InvestigateState(),
            new SearchState(),
            new ReturnState(),
            new ChaseState(),
            new AttackState()
        };

        private readonly List<Vector3> _patrol = new List<Vector3>();

        private IEnemyState _current;
        private int _patrolIndex;

        public EnemyBrain(EnemyDefinition definition, IEnemyBody body)
        {
            Definition = definition;
            Body = body;
        }

        public EnemyDefinition Definition { get; }

        public IEnemyBody Body { get; }

        public EnemyStateId State { get; private set; }

        public float TimeInState { get; private set; }

        // Where patrolling resumes when there is nothing left to look into.
        public Vector3 Home { get; private set; }

        public Vector3 Interest { get; private set; }

        public float InterestLoudness { get; private set; }

        public bool HasInterest => InterestLoudness > 0f;

        public bool HasPatrol => _patrol.Count > 0;

        public void SetPatrol(IList<Vector3> points)
        {
            _patrol.Clear();
            if (points == null)
            {
                return;
            }

            for (int i = 0; i < points.Count; i++)
            {
                _patrol.Add(points[i]);
            }
        }

        public void Begin(Vector3 home)
        {
            Home = home;
            Switch(EnemyStateId.Patrol);
        }

        public void Tick(float deltaTime)
        {
            TimeInState += deltaTime;
            _current?.Tick(this, deltaTime);
        }

        public void Switch(EnemyStateId id)
        {
            _current?.Exit(this);
            State = id;
            TimeInState = 0f;
            _current = _states[(int)id];
            _current.Enter(this);
        }

        // Louder wins. A quieter noise never pulls the enemy off what it is already going to look at.
        public void Hear(Vector3 position, float perceived)
        {
            if (perceived < Definition.PerceivedThreshold || perceived <= InterestLoudness)
            {
                return;
            }

            Interest = position;
            InterestLoudness = perceived;
        }

        // Seen rather than heard. Sight always wins: the last sighting replaces whatever noise was
        // being followed, however loud that was.
        public void Notice(Vector3 position)
        {
            Interest = position;
            InterestLoudness = 1f;
        }

        public void ForgetInterest()
        {
            InterestLoudness = 0f;
        }

        // The route is walked in order and starts over; with no route the enemy stands its post.
        public Vector3 NextPatrolPoint()
        {
            if (_patrol.Count == 0)
            {
                return Home;
            }

            Vector3 point = _patrol[_patrolIndex];
            _patrolIndex = _patrolIndex + 1 < _patrol.Count ? _patrolIndex + 1 : 0;
            return point;
        }

        public Vector3 CurrentPatrolPoint()
        {
            return _patrol.Count == 0 ? Home : _patrol[_patrolIndex];
        }
    }
}
