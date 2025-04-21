using System;
using System.Collections.Generic;

namespace UnityEngineX
{
    public class DisablableValue : EnablableDisablableValueBase
    {
        public struct Scope : IDisposable
        {
            public string Key;
            public DisablableValue DisablableValue;

            public void Dispose()
            {
                DisablableValue.RemoveDisable(Key);
            }
        }

        public DisablableValue() : base(enabledByDefault: true)
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

    public class EnablableValue : EnablableDisablableValueBase
    {
        public struct Scope : IDisposable
        {
            public string Key;
            public EnablableValue EnablableValue;

            public void Dispose()
            {
                EnablableValue.RemoveEnable(Key);
            }
        }

        public EnablableValue() : base(enabledByDefault: false)
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

    public abstract class EnablableDisablableValueBase
    {
        public event Action<bool> ValueChanged;

        private List<string> _keys = new List<string>();
        private bool _enabledByDefault = true;
        private bool _previousState;

        protected EnablableDisablableValueBase(bool enabledByDefault)
        {
            _enabledByDefault = enabledByDefault;
        }

        protected void AddKey(string key)
        {
            PreChange();

            _keys.Add(key);

            PostChange();
        }

        protected bool RemoveKey(string key)
        {
            PreChange();

            bool result = _keys.Remove(key);

            if (result)
            {
                PostChange();
            }

            return result;
        }

        public bool HasKey(string key)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (string.Equals(_keys[i], key))
                    return true;
            }
            return false;
        }

        protected int RemoveAllKeys(string key)
        {
            PreChange();

            int removeCount = 0;
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_keys[i], key))
                {
                    _keys.RemoveAt(i);
                    removeCount++;
                }
            }

            if (removeCount > 0)
            {
                PostChange();
            }

            return removeCount;
        }

        protected bool AddUniqueKey(string key)
        {
            if (!_keys.Contains(key))
            {
                AddKey(key);
                return true;
            }
            return false;
        }

        private void PreChange()
        {
            _previousState = Enabled;
        }

        private void PostChange()
        {
            bool newState = Enabled;

            if (_previousState != newState)
            {
                ValueChanged?.Invoke(newState);
            }
        }

        public bool Enabled
        {
            get => (_keys.Count == 0) == _enabledByDefault;
        }

        public override string ToString()
        {
            return Enabled.ToString();
        }

        public static implicit operator bool(EnablableDisablableValueBase val)
        {
            return val.Enabled;
        }
    }
}