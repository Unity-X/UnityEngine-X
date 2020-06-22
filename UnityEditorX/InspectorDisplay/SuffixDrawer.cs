using UnityEngine;
using UnityEditor;
using UnityEngineX.InspectorDisplay;

namespace UnityEditorX.InspectorDisplay
{
    [CustomPropertyDrawer(typeof(Suffix))]
    public class SuffixDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Suffix suffix = attribute as Suffix;

            var suffixWidth = EditorStyles.label.CalcSize(new GUIContent(suffix.text)).x;

            position.xMax -= suffixWidth;
            EditorGUI.PropertyField(position, property, true);

            position.x += position.width;
            position.width = suffixWidth;
            EditorGUI.LabelField(position, suffix.text);
        }
    }
}