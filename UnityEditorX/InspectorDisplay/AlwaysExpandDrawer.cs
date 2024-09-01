using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngineX.InspectorDisplay;

namespace UnityEditorX.InspectorDisplay
{
    [CustomPropertyDrawer(typeof(AlwaysExpandAttribute))]
    public class AlwaysExpandDrawer : PropertyDrawer
    {
        GUIContent _cachedLabel = new GUIContent();
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0;
            _cachedLabel.text = label.text;

            foreach (SerializedProperty child in new PropertyDirectChildEnumerator(property))
            {
                if (ShouldUseRealPropertyName)
                    _cachedLabel.text = property.displayName;

                totalHeight += EditorGUI.GetPropertyHeight(property, _cachedLabel, child.hasVisibleChildren) + 2 /*padding*/;
            }
            return totalHeight - 2;  // we have to subtract 2px of extra padding because the last child doesn't need it
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = false; // needed to prevent flickering in unity's drawing

            _cachedLabel.text = label.text;

            foreach (SerializedProperty child in new PropertyDirectChildEnumerator(property))
            {
                if (ShouldUseRealPropertyName)
                    _cachedLabel.text = property.displayName;

                position.height = EditorGUI.GetPropertyHeight(property, _cachedLabel, child.hasVisibleChildren);
                EditorGUI.PropertyField(position, property, _cachedLabel, true);
                position.y += position.height + 2 /*padding*/;
            }
        }

        bool ShouldUseRealPropertyName => ((AlwaysExpandAttribute)attribute).UseRootPropertyName == false;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            foreach (SerializedProperty child in new PropertyDirectChildEnumerator(property))
            {
                string label = ShouldUseRealPropertyName ? child.displayName : property.displayName;
                container.Add(new PropertyField(child, label));
            }
            return container;
        }
    }
}