using System;
using UnityEngine;

namespace DarkDepths.Core
{
    // What hunts the player needs to know about them, and nothing more. Registered in Services by the
    // player, so an enemy never searches the scene for one.
    public interface IPlayerTarget : IDamageable
    {
        Vector3 Position { get; }

        // Eye height rather than the floor: what is visible is the player, not their feet.
        Vector3 EyePosition { get; }

        // With the flashlight on the player is seen from further away (docs/05).
        bool IsLit { get; }

        // How many enemies are currently chasing; the screen shows it, as docs/04 asks.
        bool IsHunted { get; }

        event Action Died;

        void AddHunter();

        void RemoveHunter();
    }
}
