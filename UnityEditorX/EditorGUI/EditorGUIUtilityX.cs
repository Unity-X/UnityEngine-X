using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditorX
{
    /// <summary>
    /// Extras to <see cref="EditorGUIUtility"/>
    /// </summary>
    public static class EditorGUIUtilityX
    {
        private static readonly GUIContent s_text = new GUIContent();
        private static readonly GUIContent s_image = new GUIContent();
        private static readonly GUIContent s_textImage = new GUIContent();

        public static GUIContent TempContent(string t)
        {
            s_text.image = null;
            s_text.text = t;
            s_text.tooltip = null;
            return s_text;
        }

        public static GUIContent TempContent(Texture i)
        {
            s_image.image = i;
            s_image.text = null;
            s_image.tooltip = null;
            return s_image;
        }

        public static GUIContent TempContent(string t, Texture i)
        {
            s_textImage.image = i;
            s_textImage.text = t;
            s_textImage.tooltip = null;
            return s_textImage;
        }
    }
}