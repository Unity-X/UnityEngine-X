using UnityEngine;

namespace CCC.InspectorDisplay
{
    public class Suffix : PropertyAttribute
    {
        public string text;
        public Suffix(string text)
        {
            this.text = text;
        }
    }
}