using System.Collections.Generic;
using UnityEngine;

namespace UnityLLMAvatar.Utility
{
    public abstract class RuntimeScriptableObject : ScriptableObject
    {
        private static readonly List<RuntimeScriptableObject> _instances = new();

        private void OnEnable() => _instances.Add(this);
        private void OnDisable() => _instances.Remove(this);

        protected abstract void OnReset();
        
        /// <summary>
        /// Resets all instances of RuntimeScriptableObject before any scene is loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetAllInstances()
        {
            foreach (var instance in _instances)
            {
                instance.OnReset();
            }
        }
    }
}