using UnityEngine;

namespace UnityEngineX.InspectorDisplay
{
    public class Suffix : PropertyAttribute
    {
        public string Text;

        public Suffix(string text)
        {
            Text = text;
        }
    }
}