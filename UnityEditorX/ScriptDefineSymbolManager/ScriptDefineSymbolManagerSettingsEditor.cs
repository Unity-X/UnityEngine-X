using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngineX;

namespace UnityEditorX
{
    public class ScriptDefineSymbolManagerSettingsEditor
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Script Define Symbols", SettingsScope.Project)
            {
                guiHandler = OnGUI,
                keywords = new HashSet<string>(new[] { "Symbol", "Symbols", "Define", "Manager", "Script", "Scripting", "Profile", "Build" })
            };

            return provider;
        }

        private static Rect s_createProfileButtonRect;
        private static Rect s_createSymbolButtonRect;
        private static Dictionary<ScriptDefineSymbolManager.IProfile, bool> s_profileFoldStates = new Dictionary<ScriptDefineSymbolManager.IProfile, bool>();
        private static Dictionary<ScriptDefineSymbolManager.ISymbol, bool> s_symbolFoldStates = new Dictionary<ScriptDefineSymbolManager.ISymbol, bool>();

        private static void OnGUI(string searchContext)
        {
            //cleanup
            foreach (var profile in s_profileFoldStates.Keys)
            {
                if (!ScriptDefineSymbolManager.Profiles.Contains(profile))
                {
                    s_profileFoldStates.Remove(profile);
                    break;
                }
            }

            DrawProfileList();
            DrawSymbolList();
        }

        private static void DrawProfileList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Profiles", EditorStyles.boldLabel);

            string[] currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            Array.Sort(currentSymbols);

            foreach (ScriptDefineSymbolManager.IProfile profile in ScriptDefineSymbolManager.Profiles)
            {
                if (!s_profileFoldStates.ContainsKey(profile))
                    s_profileFoldStates.Add(profile, true);

                string label = profile.Name;

                string[] definedSymbols = profile.DefinedSymbols.ToArray();
                Array.Sort(definedSymbols);

                if (currentSymbols.SequenceEqual(definedSymbols))
                {
                    label += "    <color=green>✓ <i>current</i></color>";
                }
                s_profileFoldStates[profile] = EditorGUILayout.BeginFoldoutHeaderGroup(s_profileFoldStates[profile], label, EditorStylesX.FoldoutHeaderRich, menuAction: (Rect rect) =>
                {
                    Rect screenRect = GUIUtility.GUIToScreenRect(rect);
                    var genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        if (EditorUtility.DisplayDialog("Delete Profile", $"Are you sure you want to delete the profile \"{profile.Name}\" ?", "Yes", "Cancel"))
                        {
                            ScriptDefineSymbolManager.DeleteProfile(profile);
                        }
                    });
                    genericMenu.AddItem(new GUIContent("Rename"), false, () =>
                    {
                        PopupRenameProfile.Get(screenRect).Profile = profile;
                    });
                    genericMenu.AddItem(new GUIContent("Apply To Current Build Target"), false, () =>
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, profile.GetCombinedSymbols());
                    });
                    genericMenu.ShowAsContext();
                });


                if (s_profileFoldStates[profile])
                {
                    using var _0 = ListPool<string>.Get(out List<string> symbolsToAdd);
                    using var _1 = ListPool<string>.Get(out List<string> symbolsToRemove);

                    // gui each symbol
                    foreach (ScriptDefineSymbolManager.ISymbol symbol in ScriptDefineSymbolManager.GetSymbols())
                    {
                        bool enabled = profile.DefinedSymbols.Contains(symbol.Name);

                        bool newEnabled = GUILayout.Toggle(enabled, symbol.Name);

                        if (enabled != newEnabled)
                        {
                            if (newEnabled)
                                symbolsToAdd.Add(symbol.Name);
                            else
                                symbolsToRemove.Add(symbol.Name);
                        }
                    }

                    // add/remove symbols
                    foreach (var item in symbolsToAdd)
                    {
                        ScriptDefineSymbolManager.AddSymbolInProfile(item, profile);
                    }
                    foreach (var item in symbolsToRemove)
                    {
                        ScriptDefineSymbolManager.RemoveSymbolFromProfile(item, profile);
                    }

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (GUILayout.Button("New Profile"))
            {
                PopupCreateProfile.Get(s_createProfileButtonRect);
            }

            if (Event.current.type == EventType.Repaint) s_createProfileButtonRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());

            EditorGUILayout.EndVertical();
        }

        private static void DrawSymbolList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Symbols", EditorStyles.boldLabel);

            {
                using var _ = ListPool<string>.Get(out List<string> symbolsToRemove);
                foreach (var symbol in ScriptDefineSymbolManager.GetSymbols())
                {
                    if (!s_symbolFoldStates.ContainsKey(symbol))
                        s_symbolFoldStates.Add(symbol, false);

                    s_symbolFoldStates[symbol] = EditorGUILayout.BeginFoldoutHeaderGroup(s_symbolFoldStates[symbol], symbol.Name, menuAction: (Rect rect) =>
                    {
                        Rect screenRect = GUIUtility.GUIToScreenRect(rect);
                        var genericMenu = new GenericMenu();

                        if (symbol.ProvidedByCode)
                        {
                            genericMenu.AddDisabledItem(new GUIContent("Delete"));
                            genericMenu.AddDisabledItem(new GUIContent("Edit"));
                        }
                        else
                        {
                            genericMenu.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                if (EditorUtility.DisplayDialog("Delete Symbol", $"Are you sure you want to delete the symbol \"{symbol}\" ?", "Yes", "Cancel"))
                                {
                                    ScriptDefineSymbolManager.DeleteSymbol(symbol.Name);
                                }
                            });
                            genericMenu.AddItem(new GUIContent("Edit"), false, () =>
                            {
                                PopupEditSymbol.Get(screenRect, symbol.Name, symbol.Description);
                            });
                        }
                        genericMenu.ShowAsContext();
                    });

                    if (s_symbolFoldStates[symbol])
                    {
                        if (symbol.ProvidedByCode)
                        {
                            var content = EditorGUIUtilityX.TempContent($"(provided by '{symbol.CodeAssembly.GetName().Name}' assembly)");
                            var size = EditorStyles.miniLabel.CalcSize(content);
                            EditorGUILayout.LabelField(content, EditorStyles.miniLabel, GUILayout.Width(size.x));
                        }

                        EditorGUI.indentLevel++;
                        if (!string.IsNullOrEmpty(symbol.Description))
                        {
                            EditorGUILayout.LabelField(symbol.Description, EditorStylesX.LongText);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("No description", EditorStylesX.LongText);

                        }
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                foreach (var symbol in symbolsToRemove)
                {
                    ScriptDefineSymbolManager.DeleteSymbol(symbol);
                }
            }

            if (GUILayout.Button("New Symbol"))
            {
                PopupCreateSymbol.Get(s_createSymbolButtonRect);
            }

            if (Event.current.type == EventType.Repaint) s_createSymbolButtonRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());


            EditorGUILayout.EndVertical();
        }

        public class PopupCreateProfile : EditorWindow
        {
            public static PopupCreateProfile Get(Rect buttonRect)
            {
                Rect rect = new Rect(buttonRect.position + Vector2.up * 60, new Vector2(275, 45));
                var window = GetWindowWithRect<PopupCreateProfile>(rect, true, "Create Profile");
                window.position = rect;
                return window;
            }

            private string _name;

            public void OnGUI()
            {
                _name = EditorGUILayout.TextField("Name", _name);

                if (GUILayout.Button("Create"))
                {
                    ScriptDefineSymbolManager.CreateProfile(_name);
                    Close();
                }
            }

            private void OnLostFocus()
            {
                Close();
            }
        }

        public class PopupRenameProfile : EditorWindow
        {
            public static PopupRenameProfile Get(Rect buttonRect)
            {
                Rect p = new Rect(buttonRect.position, new Vector2(275, 45));
                var window = GetWindowWithRect<PopupRenameProfile>(p, true, "Rename Profile");
                window.position = p;

                return window;
            }

            public ScriptDefineSymbolManager.IProfile Profile;

            private string _name;
            private bool _firstUpdate = true;

            public void OnGUI()
            {
                if (_firstUpdate)
                {
                    _name = Profile.Name;
                    _firstUpdate = false;
                }

                _name = EditorGUILayout.TextField("Name", _name);

                if (GUILayout.Button("Rename"))
                {
                    ScriptDefineSymbolManager.RenameProfile(Profile, _name);
                    Close();
                }
            }

            private void OnLostFocus()
            {
                Close();
            }
        }

        public class PopupCreateSymbol : EditorWindow
        {
            public static PopupCreateSymbol Get(Rect buttonRect)
            {
                Rect rect = new Rect(buttonRect.position + Vector2.up * 60, new Vector2(375, 167));
                var window = GetWindowWithRect<PopupCreateSymbol>(rect, true, "Create Symbol");
                window.position = rect;

                return window;
            }

            private string _name;
            private string _description;

            public void OnGUI()
            {
                _name = EditorGUILayout.TextField("Name", _name);
                EditorGUILayout.LabelField("Description");
                _description = EditorGUILayout.TextArea(_description, EditorStylesX.TextAreaWrap, GUILayout.Height(100));

                if (GUILayout.Button("Save"))
                {
                    ScriptDefineSymbolManager.CreateSymbol(_name, _description);
                    Close();
                }
            }

            private void OnLostFocus()
            {
                Close();
            }
        }

        public class PopupEditSymbol : EditorWindow
        {
            public static PopupEditSymbol Get(Rect buttonRect, string currentSymbolName, string currentDescription)
            {
                Rect rect = new Rect(buttonRect.position + Vector2.up * 60, new Vector2(375, 167));
                var window = GetWindowWithRect<PopupEditSymbol>(rect, true, "Edit Symbol");
                window.position = rect;

                window.CurrentSymbolName = currentSymbolName;
                window._name = currentSymbolName;
                window._description = currentDescription;

                return window;
            }

            public string CurrentSymbolName;
            private string _name;
            private string _description;

            public void OnGUI()
            {
                _name = EditorGUILayout.TextField("Name", _name);
                EditorGUILayout.LabelField("Description");
                _description = EditorGUILayout.TextArea(_description, EditorStylesX.TextAreaWrap, GUILayout.Height(100));

                if (GUILayout.Button("Save"))
                {
                    ScriptDefineSymbolManager.EditSymbol(CurrentSymbolName, _name, _description);
                    Close();
                }
            }

            private void OnLostFocus()
            {
                Close();
            }
        }
    }
}