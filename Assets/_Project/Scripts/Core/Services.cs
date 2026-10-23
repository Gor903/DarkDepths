using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkDepths.Core
{
    // The one locator docs/07 allows. A service registers itself, everything else asks for the interface
    // and caches the answer outside of Update.
    public static class Services
    {
        private static readonly Dictionary<Type, object> Registry = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Registry.Remove(typeof(T));
                return;
            }

            Registry[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class
        {
            Registry.Remove(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            return Registry.TryGetValue(typeof(T), out object service) ? service as T : null;
        }

        public static bool Has<T>() where T : class
        {
            return Registry.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            Registry.Clear();
        }

        // Entering play mode with domain reload turned off would otherwise keep the previous run's services.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Clear();
        }
    }
}
