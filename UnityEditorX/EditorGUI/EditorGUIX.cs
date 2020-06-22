using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

// Disable Naming Styles warnings: We are intentionaly mimicking unity's source code, which doesn't respect our naming conventions
#pragma warning disable IDE1006

namespace UnityEditorX
{
    /// <summary>
    /// Extras to <see cref="EditorGUI"/>
    /// </summary>
    public static partial class EditorGUIX
    {
        public const float kSingleLineHeight = 18f;
        internal const float kSpacing = 5;

        // sealed partial class for storing state for popup menus so we can get the info back to OnGUI from the user selection
        internal sealed class PopupCallbackInfo
        {
            // The global shared popup state
            public static PopupCallbackInfo instance = null;

            // Name of the command event sent from the popup menu to OnGUI when user has changed selection
            internal const string kPopupMenuChangedMessage = "PopupMenuChanged";

            // The control ID of the popup menu that is currently displayed.
            // Used to pass selection changes back again...
            private readonly int m_ControlID = 0;

            // Which item was selected
            private int m_SelectedIndex = 0;

            // Which view should we send it to.
            private readonly EditorWindow m_SourceView;

            // *undoc*
            public PopupCallbackInfo(int controlID)
            {
                m_ControlID = controlID;
                m_SourceView = EditorWindow.focusedWindow;
            }

            // *undoc*
            public static int GetSelectedValueForControl(int controlID, int selected)
            {
                Event evt = Event.current;
                if (evt.type == EventType.ExecuteCommand && evt.commandName == kPopupMenuChangedMessage)
                {
                    if (instance == null)
                    {
                        Debug.LogError("Popup menu has no instance");
                        return selected;
                    }
                    if (instance.m_ControlID == controlID)
                    {
                        selected = instance.m_SelectedIndex;
                        instance = null;
                        GUI.changed = true;
                        evt.Use();
                    }
                }
                return selected;
            }

            internal void SetEnumValueDelegate(object userData, string[] options, int selected)
            {
                m_SelectedIndex = selected;
                if (m_SourceView)
                {
                    m_SourceView.SendEvent(EditorGUIUtility.CommandEvent(kPopupMenuChangedMessage));
                }
            }
        }

        private static readonly int s_searchablePopupHash = "SearchableEditorPopup".GetHashCode();
        private static readonly GUIContent s_mixedValueContent = EditorGUIUtility.TrTextContent("\u2014", "Mixed Values");

        private static readonly Color s_mixedValueContentColor = new Color(1, 1, 1, 0.5f);
        private static Color s_mixedValueContentColorTemp = Color.white;

        private static void BeginHandleMixedValueContentColor()
        {
            s_mixedValueContentColorTemp = GUI.contentColor;
            GUI.contentColor = EditorGUI.showMixedValue ? (GUI.contentColor * s_mixedValueContentColor) : GUI.contentColor;
        }

        private static void EndHandleMixedValueContentColor()
        {
            GUI.contentColor = s_mixedValueContentColorTemp;
        }

        // Use this method when checking if user hit Space or Return in order to activate the main action
        // for a control, such as opening a popup menu or color picker.
        private static bool MainActionKeyForControl(this UnityEngine.Event evt, int controlId)
        {
            if (EditorGUIUtility.keyboardControl != controlId)
                return false;

            bool anyModifiers = (evt.alt || evt.shift || evt.command || evt.control);

            // Space or return is action key
            return evt.type == EventType.KeyDown &&
                (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) &&
                !anyModifiers;
        }
    }



    /// <summary>
    /// Extras to <see cref="EditorGUILayout"/>
    /// </summary>
    public static partial class EditorGUILayoutX
    {
        internal static float kLabelFloatMinW => EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + EditorGUIX.kSpacing;
        internal static float kLabelFloatMaxW => EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + EditorGUIX.kSpacing;

        private static FieldInfo s_LastRectField;

        private static Rect SetLastRect(Rect rect)
        {
            if (s_LastRectField == null)
            {
                s_LastRectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.NonPublic | BindingFlags.Static);
            }
            s_LastRectField.SetValue(null, rect);

            return rect;
        }

        public static Rect GetControlRect(params GUILayoutOption[] options)
        {
            return GetControlRect(true, EditorGUIX.kSingleLineHeight, EditorStyles.layerMaskField, options);
        }

        public static Rect GetControlRect(bool hasLabel, params GUILayoutOption[] options)
        {
            return GetControlRect(hasLabel, EditorGUIX.kSingleLineHeight, EditorStyles.layerMaskField, options);
        }

        public static Rect GetControlRect(bool hasLabel, float height, params GUILayoutOption[] options)
        {
            return GetControlRect(hasLabel, height, EditorStyles.layerMaskField, options);
        }

        public static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayoutUtility.GetRect(
                hasLabel ? kLabelFloatMinW : EditorGUIUtility.fieldWidth,
                kLabelFloatMaxW,
                height, height, style, options);
        }

        public static bool XButton()
        {
            return GUILayout.Button("✕", EditorStylesX.MiniXButton);
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles