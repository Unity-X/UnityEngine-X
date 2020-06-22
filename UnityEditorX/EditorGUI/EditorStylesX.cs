using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditorX
{
    /// <summary>
    /// Extras to <see cref="EditorStyles"/>
    /// </summary>
    public static partial class EditorStylesX
    {
        private static GUIStyle s_miniXButton;

        public static GUIStyle MiniXButton
        {
            get
            {
                if (s_miniXButton == null)
                {
                    s_miniXButton = new GUIStyle(EditorStyles.miniButton);
                    s_miniXButton.normal.textColor = new Color(0.3f, 0.3f, 0.3f, 1);

                    s_miniXButton.margin.top = 3;
                    s_miniXButton.margin.bottom = 3;
                    s_miniXButton.margin.right = 3;
                    s_miniXButton.margin.left = 3;

                    s_miniXButton.padding.top = 0;
                    s_miniXButton.padding.left = 0;
                    s_miniXButton.padding.right = 0;
                    s_miniXButton.padding.bottom = 0;

                    s_miniXButton.contentOffset = new Vector2(0, -1);

                    s_miniXButton.fontSize = 10;
                    s_miniXButton.fixedHeight = 16;
                    s_miniXButton.fixedWidth = 16;
                }

                return s_miniXButton;
            }
        }

        private static GUIStyle s_longText;

        public static GUIStyle LongText
        {
            get
            {
                if (s_longText == null)
                {
                    s_longText = new GUIStyle(EditorStyles.label);
                    s_longText.normal.textColor = new Color(0.22f, 0.22f, 0.22f, 1);
                    s_longText.name = "Long Text";
                    s_longText.wordWrap = true;
                }

                return s_longText;
            }
        }

        private static GUIStyle s_textAreaWrap;

        public static GUIStyle TextAreaWrap
        {
            get
            {
                if (s_textAreaWrap == null)
                {
                    s_textAreaWrap = new GUIStyle(EditorStyles.textArea);
                    s_textAreaWrap.wordWrap = true;
                }

                return s_textAreaWrap;
            }
        }

        private static GUIStyle s_foldoutHeaderRichWrap;

        public static GUIStyle FoldoutHeaderRich
        {
            get
            {
                if (s_foldoutHeaderRichWrap == null)
                {
                    s_foldoutHeaderRichWrap = new GUIStyle(EditorStyles.foldoutHeader);
                    s_foldoutHeaderRichWrap.richText = true;
                }

                return s_foldoutHeaderRichWrap;
            }
        }
    }
}