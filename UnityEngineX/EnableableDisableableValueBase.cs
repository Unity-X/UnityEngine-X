using System;
using System.Collections.Generic;

namespace UnityEngineX
{
    public abstract class EnableableDisableableValueBase
    {
        public event Action<bool> ValueChanged;

        private List<string> _keys = new List<string>();
        private bool _enabledByDefault = true;
        private bool _previousState;

        protected EnableableDisableableValueBase(bool enabledByDefault)
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

        public static implicit operator bool(EnableableDisableableValueBase val)
        {
            return val.Enabled;
        }
    }
}