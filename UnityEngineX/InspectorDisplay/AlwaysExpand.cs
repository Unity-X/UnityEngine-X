using System;
using UnityEngine;

namespace UnityEngineX.InspectorDisplay
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AlwaysExpandAttribute : PropertyAttribute
    {
        public AlwaysExpandAttribute() { }

        public bool UseRootPropertyName = false;
    }
}