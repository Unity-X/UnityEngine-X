using System;

namespace UnityEngineX
{
    public class EnableableValue : EnableableDisableableValueBase
    {
        public struct Scope : IDisposable
        {
            public string Key;
            public EnableableValue EnablableValue;

            public void Dispose()
            {
                EnablableValue.RemoveEnable(Key);
            }
        }

        public EnableableValue() : base(enabledByDefault: false)
        {
        }

        /// <summary>
        /// Add an enable, enabling the value if it was previously disabled.
        /// </summary>
        public Scope AddEnable(string key)
        {
            AddKey(key);
            return new() { EnablableValue = this, Key = key };
        }

        /// <summary>
        /// Add an enable if the given key was not already there. Returns true if the enable was added.
        /// </summary>
        public bool AddUniqueEnable(string key) => AddUniqueKey(key);

        /// <summary>
        /// Is the given enable found at least once?
        /// </summary>
        public bool HasEnable(string key) => HasKey(key);

        /// <summary>
        /// Remove an enable, disabling the value if no enables are left. Returns true if the enable was found and removed.
        /// </summary>
        public bool RemoveEnable(string key) => RemoveKey(key);

        /// <summary>
        /// Remove all instances of the given enable.
        /// </summary>
        public int RemoveAllEnables(string key) => RemoveAllKeys(key);
    }
}