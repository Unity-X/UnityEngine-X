using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace UnityEditorX
{
    public class SearchablePopupWindow : EditorWindow
    {
        private class Styles
        {
#pragma warning disable IDE1006 // Naming Styles
            public static GUIStyle textStyle;
            public static GUIStyle selectedBackgroundStyle;
            public static GUIStyle normalBackgroundStyle;
            public static GUIStyle searchToobarStyle;
            public static GUIStyle background = "DD Background";
            public static bool styleInitialized = false;
#pragma warning restore IDE1006 // Naming Styles
        }

        private Vector2 _scrollViewPosition;
        private int _selectedDisplayedOptionIndex;
        private string[] _options;
        private List<int> _displayedOptionIndexes = new List<int>();

        private bool _submitNewSelection = false;
        private bool _closeWindow = false;

        private string _searchText = string.Empty;
        private Func<int, bool> _checkEnabled;
        private int _selected;
        private EditorUtility.SelectMenuItemFunction _callback;
        private object _userData;

        public void Init(string[] options, Func<int, bool> checkEnabled, int selected, EditorUtility.SelectMenuItemFunction callback, object userData)
        {
            _options = options;
            _checkEnabled = checkEnabled;
            _selected = selected;
            _callback = callback;
            _userData = userData;

            _searchText = string.Empty;
            _scrollViewPosition.y = 16 * Mathf.Max(0, selected);
            _submitNewSelection = false;

            UpdateDisplayedElementsList();
        }

        private static void InitStyle()
        {
            if (!Styles.styleInitialized)
            {
                Styles.searchToobarStyle = EditorStyles.toolbarSearchField;

                Styles.textStyle = new GUIStyle(EditorStyles.label);
                Styles.textStyle.fixedHeight = 16;
                Styles.textStyle.alignment = TextAnchor.MiddleLeft;

                Texture2D selectedBg = new Texture2D(32, 32, TextureFormat.RGB24, false);
                Texture2D hightLightBg = new Texture2D(32, 32, TextureFormat.RGB24, false);
                if (EditorGUIUtility.isProSkin)
                {
                    selectedBg.LoadImage(System.Convert.FromBase64String(SELECTEDBG_PRO));
                    hightLightBg.LoadImage(System.Convert.FromBase64String(HIGHTLIGHTBG_PRO));
                }
                else
                {
                    selectedBg.LoadImage(System.Convert.FromBase64String(SELECTEDBG_LIGHT));
                    hightLightBg.LoadImage(System.Convert.FromBase64String(HIGHTLIGHTBG_LIGHT));
                }

                Styles.selectedBackgroundStyle = new GUIStyle();
                Styles.selectedBackgroundStyle.normal.background = selectedBg;
                Styles.normalBackgroundStyle = new GUIStyle();
                Styles.normalBackgroundStyle.hover.background = hightLightBg;

                Styles.styleInitialized = true;
            }
        }

        void OnGUI()
        {
            InitStyle();

            KeyCode keyDown = KeyCode.None;
            if (Event.current.type == EventType.KeyDown)
            {
                keyDown = Event.current.keyCode;
            }

            GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, Styles.background);

            DrawSearchField();

            DrawDisplayedOption();

            HandleInput(keyDown);

            if (_submitNewSelection)
            {
                _callback.Invoke(_userData, _options, _selected);
                _submitNewSelection = false;
            }

            if (_closeWindow)
            {
                _closeWindow = false;
                Close();
            }
        }

        void DrawSearchField()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            var previousBGColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
            GUI.SetNextControlName("Search");

            var oldSearchText = _searchText;
            _searchText = EditorGUILayout.TextField("", _searchText, Styles.searchToobarStyle, GUILayout.MinWidth(95));

            if (_searchText != oldSearchText)
            {
                UpdateDisplayedElementsList();
            }

            EditorGUI.FocusTextInControl("Search");
            GUILayout.EndHorizontal();
            GUI.backgroundColor = previousBGColor;
        }

        void DrawDisplayedOption()
        {
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
            _selectedDisplayedOptionIndex = -1;
            for (int i = 0; i < _displayedOptionIndexes.Count; i++)
            {
                bool isSelected = (_displayedOptionIndexes[i] == _selected);

                if (isSelected)
                {
                    _selectedDisplayedOptionIndex = i;
                }

                Rect rect = EditorGUILayout.BeginHorizontal(isSelected ? Styles.selectedBackgroundStyle : Styles.normalBackgroundStyle);

                GUILayout.Label(_options[_displayedOptionIndexes[i]], Styles.textStyle);
                GUILayout.FlexibleSpace();

                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    _selected = _displayedOptionIndexes[i];
                    _submitNewSelection = true;
                    _closeWindow = true;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        void HandleInput(KeyCode keyDown)
        {
            if (keyDown == KeyCode.UpArrow)
            {
                if (_selectedDisplayedOptionIndex > 0)
                {
                    _selected = _displayedOptionIndexes[_selectedDisplayedOptionIndex - 1];
                }
            }
            if (keyDown == KeyCode.DownArrow)
            {
                if (_selectedDisplayedOptionIndex < _displayedOptionIndexes.Count - 1)
                {
                    _selected = _displayedOptionIndexes[_selectedDisplayedOptionIndex + 1];
                }
            }
            if (keyDown == KeyCode.Escape)
            {
                _closeWindow = true;
            }
            if (keyDown == KeyCode.KeypadEnter || keyDown == KeyCode.Return)
            {
                _submitNewSelection = true;
                _closeWindow = true;
            }
        }

        void UpdateDisplayedElementsList()
        {
            _displayedOptionIndexes.Clear();

            for (int i = 0; i < _options.Length; i++)
            {
                if (!string.IsNullOrEmpty(_searchText) && !_options[i].ToLower().Contains(_searchText))
                {
                    continue;
                }

                _displayedOptionIndexes.Add(i);
            }
        }

        void Update()
        {
            Repaint();
        }

        const string SELECTEDBG_PRO = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAQklEQVRIDe3SsQkAAAgDQXWN7L+nOMFXdm8dIhzpJPV581l+3T5AYYkkQgEMuCKJUAADrkgiFMCAK5IIBTDgipBoAWXpAJEoZnl3AAAAAElFTkSuQmCC";

        const string HIGHTLIGHTBG_PRO = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAQklEQVRIDe3SsQkAAAgDQXXD7L+MOMFXdm8dIhzpJPV581l+3T5AYYkkQgEMuCKJUAADrkgiFMCAK5IIBTDgipBoARFdATMHrayuAAAAAElFTkSuQmCC";

        const string SELECTEDBG_LIGHT = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAQUlEQVRIDe3SsQkAAAgDQXV/yMriBF/ZvXWIcKST1OfNZ/l1+wCFJZIIBTDgiiRCAQy4IolQAAOuSCIUwIArQqIF36EB7diYDg8AAAAASUVORK5CYII=";

        const string HIGHTLIGHTBG_LIGHT = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAQklEQVRIDe3SsQkAAAgDQXX/ETOMOMFXdm8dIhzpJPV581l+3T5AYYkkQgEMuCKJUAADrkgiFMCAK5IIBTDgipBoAc9YAtQLJ3kPAAAAAElFTkSuQmCC";
    }

    public static partial class EditorUtilityX
    {
        public static void DisplaySearchableCustomMenu(
            Rect position,
            string[] options,
            Func<int, bool> checkEnabled,
            int selected,
            EditorUtility.SelectMenuItemFunction callback,
            object userData,
            float windowHeight = 300)
        {
            var window = EditorWindow.CreateInstance<SearchablePopupWindow>();
            window.Init(options, checkEnabled, selected, callback, userData);

            position.position = GUIUtility.GUIToScreenPoint(position.position);
            position.height += 1;
            window.ShowAsDropDown(position, new Vector2(position.width, windowHeight));
        }
    }

    public static partial class EditorGUIX
    {
        private static int SearchablePopupInternal(Rect position, GUIContent label, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style)
        {
            int id = GUIUtility.GetControlID(s_searchablePopupHash, FocusType.Keyboard, position);
            if (label != null)
                position = EditorGUI.PrefixLabel(position, id, label);
            return DoSearchablePopup(position, id, selectedIndex, displayedOptions, checkEnabled, style);
        }

        private static int DoSearchablePopup(Rect position, int controlID, int selected, string[] popupValues, Func<int, bool> checkEnabled, GUIStyle style)
        {
            selected = PopupCallbackInfo.GetSelectedValueForControl(controlID, selected);

            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.Repaint:

                    GUIContent buttonContent;
                    if (EditorGUI.showMixedValue)
                    {
                        buttonContent = s_mixedValueContent;
                    }
                    else if (selected < 0 || selected >= popupValues.Length)
                    {
                        buttonContent = GUIContent.none;
                    }
                    else
                    {
                        buttonContent = EditorGUIUtilityX.TempContent(popupValues[selected]);
                    }

                    BeginHandleMixedValueContentColor();
                    style.Draw(position, buttonContent, controlID, false, position.Contains(Event.current.mousePosition));
                    EndHandleMixedValueContentColor();
                    break;

                case EventType.MouseDown:
                    if (evt.button == 0 && position.Contains(evt.mousePosition))
                    {
                        if (Application.platform == RuntimePlatform.OSXEditor)
                        {
                            position.y = position.y - selected * 16 - 19;
                        }

                        PopupCallbackInfo.s_instance = new PopupCallbackInfo(controlID);
                        EditorUtilityX.DisplaySearchableCustomMenu(position, popupValues, checkEnabled, EditorGUI.showMixedValue ? -1 : selected, PopupCallbackInfo.s_instance.SetEnumValueDelegate, null);
                        GUIUtility.keyboardControl = controlID;
                        evt.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (evt.MainActionKeyForControl(controlID))
                    {
                        if (Application.platform == RuntimePlatform.OSXEditor)
                        {
                            position.y = position.y - selected * 16 - 19;
                        }

                        PopupCallbackInfo.s_instance = new PopupCallbackInfo(controlID);
                        EditorUtilityX.DisplaySearchableCustomMenu(position, popupValues, checkEnabled, EditorGUI.showMixedValue ? -1 : selected, PopupCallbackInfo.s_instance.SetEnumValueDelegate, null);
                        evt.Use();
                    }
                    break;
            }
            return selected;
        }

        public static int SearchablePopup(Rect position, int selectedIndex, string[] displayedOptions)
        {
            return SearchablePopup(position, null, selectedIndex, displayedOptions, null, EditorStylesX.SearchablePopup);
        }
        public static int SearchablePopup(Rect position, int selectedIndex, string[] displayedOptions, GUIStyle style)
        {
            return SearchablePopup(position, null, selectedIndex, displayedOptions, null, style);
        }
        public static int SearchablePopup(Rect position, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled)
        {
            return SearchablePopup(position, null, selectedIndex, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup);
        }
        public static int SearchablePopup(Rect position, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style)
        {
            return SearchablePopup(position, null, selectedIndex, displayedOptions, checkEnabled, style);
        }

        public static int SearchablePopup(Rect position, GUIContent label, int selectedIndex, string[] displayedOptions)
        {
            return SearchablePopup(position, label, selectedIndex, displayedOptions, null, EditorStylesX.SearchablePopup);
        }
        public static int SearchablePopup(Rect position, GUIContent label, int selectedIndex, string[] displayedOptions, GUIStyle style)
        {
            return SearchablePopup(position, label, selectedIndex, displayedOptions, null, style);
        }
        public static int SearchablePopup(Rect position, GUIContent label, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled)
        {
            return SearchablePopup(position, label, selectedIndex, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup);
        }
        public static int SearchablePopup(Rect position, GUIContent label, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style)
        {
            return SearchablePopupInternal(position, label, selectedIndex, displayedOptions, checkEnabled, style);
        }

        public static void SearchablePopupString(Rect position, SerializedProperty property, string[] displayedOptions)
        {
            SearchablePopupString(position, property, displayedOptions, null, null);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, string[] displayedOptions, GUIStyle style)
        {
            SearchablePopupString(position, property, displayedOptions, null, style);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, string[] displayedOptions, Func<int, bool> checkEnabled)
        {
            SearchablePopupString(position, property, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style)
        {
            GUIContent label = EditorGUIUtilityX.TempContent(property.displayName);
            SearchablePopupString(position, property, label, displayedOptions, checkEnabled, style);
        }

        public static void SearchablePopupString(Rect position, SerializedProperty property, GUIContent label, string[] displayedOptions)
        {
            SearchablePopupString(position, property, label, displayedOptions, null, null);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, GUIContent label, string[] displayedOptions, GUIStyle style)
        {
            SearchablePopupString(position, property, label, displayedOptions, null, style);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, GUIContent label, string[] displayedOptions, Func<int, bool> checkEnabled)
        {
            SearchablePopupString(position, property, label, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup);
        }
        public static void SearchablePopupString(Rect position, SerializedProperty property, GUIContent label, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            int selectedIndex = Array.IndexOf(displayedOptions, property.stringValue);
            int newValue = SearchablePopup(position, label, selectedIndex, displayedOptions, checkEnabled, style);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = displayedOptions[newValue];

            EditorGUI.EndProperty();
        }

    }
    public static partial class EditorGUILayoutX
    {
        public static int SearchablePopup(int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
        {
            return SearchablePopup((GUIContent)null, selectedIndex, displayedOptions, null, EditorStylesX.SearchablePopup, options);
        }
        public static int SearchablePopup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            return SearchablePopup((GUIContent)null, selectedIndex, displayedOptions, null, style, options);
        }
        public static int SearchablePopup(int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, params GUILayoutOption[] options)
        {
            return SearchablePopup(null, selectedIndex, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup, options);
        }
        public static int SearchablePopup(int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style, params GUILayoutOption[] options)
        {
            return SearchablePopup((GUIContent)null, selectedIndex, displayedOptions, checkEnabled, style, options);
        }

        public static int SearchablePopup(GUIContent label, int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
        {
            return SearchablePopup(label, selectedIndex, displayedOptions, null, EditorStylesX.SearchablePopup, options);
        }
        public static int SearchablePopup(GUIContent label, int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            return SearchablePopup(label, selectedIndex, displayedOptions, null, style, options);
        }
        public static int SearchablePopup(GUIContent label, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, params GUILayoutOption[] options)
        {
            return SearchablePopup(label, selectedIndex, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup, options);
        }
        public static int SearchablePopup(GUIContent label, int selectedIndex, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect position = SetLastRect(GetControlRect(true, EditorGUIX.kSingleLineHeight, style, options));

            return EditorGUIX.SearchablePopup(position, label, selectedIndex, displayedOptions, checkEnabled, style);
        }

        public static void SearchablePopupString(SerializedProperty property, string[] displayedOptions, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, displayedOptions, null, EditorStylesX.SearchablePopup, options);
        }
        public static void SearchablePopupString(SerializedProperty property, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, displayedOptions, null, style, options);
        }
        public static void SearchablePopupString(SerializedProperty property, string[] displayedOptions, Func<int, bool> checkEnabled, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup, options);
        }
        public static void SearchablePopupString(SerializedProperty property, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect position = SetLastRect(GetControlRect(true, EditorGUIX.kSingleLineHeight, style, options));

            EditorGUIX.SearchablePopupString(position, property, displayedOptions, checkEnabled, style);
        }

        public static void SearchablePopupString(SerializedProperty property, GUIContent label, string[] displayedOptions, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, label, displayedOptions, null, EditorStylesX.SearchablePopup, options);
        }
        public static void SearchablePopupString(SerializedProperty property, GUIContent label, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, label, displayedOptions, null, style, options);
        }
        public static void SearchablePopupString(SerializedProperty property, GUIContent label, string[] displayedOptions, Func<int, bool> checkEnabled, params GUILayoutOption[] options)
        {
            SearchablePopupString(property, label, displayedOptions, checkEnabled, EditorStylesX.SearchablePopup, options);
        }
        public static void SearchablePopupString(SerializedProperty property, GUIContent label, string[] displayedOptions, Func<int, bool> checkEnabled, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect position = SetLastRect(GetControlRect(true, EditorGUIX.kSingleLineHeight, style, options));

            EditorGUIX.SearchablePopupString(position, property, label, displayedOptions, checkEnabled, style);
        }
    }

    public static partial class EditorStylesX
    {
        public static GUIStyle SearchablePopup => EditorStyles.miniPullDown;
    }
}