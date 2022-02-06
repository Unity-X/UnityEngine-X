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
            EditorGUI.PropertyField(position, property, label, true);

            Suffix suffix = attribute as Suffix;
            var suffixWidth = EditorStyles.miniLabel.CalcSize(new GUIContent(suffix.Text)).x;
            position.x += position.width - suffixWidth - 4;
            position.width = suffixWidth;
            EditorGUI.LabelField(position, suffix.Text, EditorStyles.miniLabel);
        }
    }
}