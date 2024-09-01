using UnityEditor;

namespace UnityEditorX
{
    public struct PropertyDirectChildEnumerator
    {
        bool _enterChildren;
        string _parentPath;
        public PropertyDirectChildEnumerator(SerializedProperty property)
        {
            Current = property;
            _enterChildren = property.hasChildren;
            _parentPath = property.propertyPath;
        }

        // Enumerator interface
        public PropertyDirectChildEnumerator GetEnumerator() => this;
        public SerializedProperty Current { get; }
        public bool MoveNext()
        {
            bool result = Current.Next(_enterChildren);

            _enterChildren = false;

            return result && Current.propertyPath.Contains(_parentPath);
        }
    }
}