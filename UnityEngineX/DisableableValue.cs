using System;

namespace UnityEngineX
{
    public class DisableableValue : EnableableDisableableValueBase
    {
        public struct Scope : IDisposable
        {
            public string Key;
            public DisableableValue DisablableValue;

            public void Dispose()
            {
                DisablableValue.RemoveDisable(Key);
            }
        }

        public DisableableValue() : base(enabledByDefault: true)
        {
        }

        /// <summary>
        /// Add a disable, disabling the value if it was previously enabled.
        /// </summary>
        public Scope AddDisable(string key)
        {
            AddKey(key);
            return new() { DisablableValue = this, Key = key };
        }

        /// <summary>
        /// Add a disable if the given key was not already there. Returns true if the disable was added.
        /// </summary>
        public bool AddUniqueDisable(string key) => AddUniqueKey(key);

        /// <summary>
        /// Is the given disable found at least once?
        /// </summary>
        public bool HasDisable(string key) => HasKey(key);

        /// <summary>
        /// Remove a disable, enabling the value if no disables are left. Returns true if the disable was found and removed.
        /// </summary>
        public bool RemoveDisable(string key) => RemoveKey(key);

        /// <summary>
        /// Remove all instances of the given disable.
        /// </summary>
        public int RemoveAllDisables(string key) => RemoveAllKeys(key);
    }
}