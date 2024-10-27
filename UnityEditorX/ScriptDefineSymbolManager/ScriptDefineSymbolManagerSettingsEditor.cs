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
        private static Dictionary<string, bool> s_profileFoldStates = new();
        private static Dictionary<ScriptDefineSymbolManager.ISymbol, bool> s_symbolFoldStates = new();

        private static void OnGUI(string searchContext)
        {
            var profiles = ScriptDefineSymbolManager.GetProfiles();

            //cleanup
            foreach (var profileName in s_profileFoldStates.Keys)
            {
                if (profiles.Find(p => p.Name == profileName) == null)
                {
                    s_profileFoldStates.Remove(profileName);
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

            foreach (ScriptDefineSymbolManager.IProfile profile in ScriptDefineSymbolManager.GetProfiles())
            {
                if (!s_profileFoldStates.ContainsKey(profile.Name))
                    s_profileFoldStates.Add(profile.Name, true);

                string label = profile.Name;

                string[] definedSymbols = profile.DefinedSymbols;
                Array.Sort(definedSymbols);

                s_profileFoldStates[profile.Name] = EditorGUILayout.BeginFoldoutHeaderGroup(s_profileFoldStates[profile.Name], label, EditorStylesX.FoldoutHeaderRich);

                if (s_profileFoldStates[profile.Name])
                {
                    using var _0 = ListPool<string>.Get(out List<string> symbolsToAdd);
                    using var _1 = ListPool<string>.Get(out List<string> symbolsToRemove);

                    // gui each symbol

                    Dictionary<string, ScriptDefineSymbolManager.ISymbol> allSymbolsIncludingMissing = new();
                    foreach (var sym in ScriptDefineSymbolManager.GetSymbols())
                    {
                        allSymbolsIncludingMissing.Add(sym.Name, sym);
                    }

                    foreach (var sym in profile.DefinedSymbols)
                    {
                        if (!allSymbolsIncludingMissing.ContainsKey(sym))
                            allSymbolsIncludingMissing.Add(sym, null);
                    }

                    foreach (var symbol in allSymbolsIncludingMissing)
                    {
                        bool enabled = profile.DefinedSymbols.Contains(symbol.Key);

                        bool missing = symbol.Value == null;

                        if (missing)
                            GUILayout.BeginHorizontal();

                        bool newEnabled = GUILayout.Toggle(enabled, symbol.Key);

                        // missing symbol!
                        if (missing)
                        {
                            var wasColor = GUI.color;
                            GUI.color = Color.yellow;
                            GUILayout.Label("MISSING SYMBOL");
                            GUI.color = wasColor;
                            GUILayout.EndHorizontal();
                        }

                        if (enabled != newEnabled)
                        {
                            if (newEnabled)
                                symbolsToAdd.Add(symbol.Key);
                            else
                                symbolsToRemove.Add(symbol.Key);
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