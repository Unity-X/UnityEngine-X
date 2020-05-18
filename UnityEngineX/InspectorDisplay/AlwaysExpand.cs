using UnityEngine;

namespace UnityEngineX.InspectorDisplay
{
    public class AlwaysExpandAttribute : PropertyAttribute
    {
        public AlwaysExpandAttribute() { }

        public bool UseRootPropertyName = false;
    }
}