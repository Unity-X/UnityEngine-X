using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;
using UnityEngineX;
using static UnityEditorX.ScriptDefineSymbolManagerSettings;

namespace UnityEditorX
{
    public static partial class ScriptDefineSymbolManager
    {
        public interface IProfile
        {
            string Name { get; }
            string[] DefinedSymbols { get; }
        }

        public interface ISymbol
        {
            string Name { get; }
            string Description { get; }
            bool ProvidedByCode { get; }
            Assembly CodeAssembly { get; }
        }

        private class CodeSymbole : ISymbol
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public bool ProvidedByCode => true;

            public Assembly CodeAssembly { get; set; }
        }

        private static List<ISymbol> s_codeProvidedSymbols = new List<ISymbol>();
        private static ScriptDefineSymbolManagerSettings s_settings;
        private static bool s_initialized;

        private class BuildProfileWrapper : IProfile
        {
            public BuildProfileWrapper(BuildProfile profile)
            {
                Profile = profile;
            }

            public string Name => Profile.name;

            public string[] DefinedSymbols => Profile.scriptingDefines;

            public BuildProfile Profile { get; private set; }
        }

        public static IProfile[] GetProfiles()
        {
            var buildProfiles = AssetDatabaseX.LoadAssetsOfType<BuildProfile>();
            return buildProfiles.Select(p => new BuildProfileWrapper(p)).ToArray();
        }

        private static void InitializeIfNeeded()
        {
            if (s_initialized)
                return;
            s_initialized = true;

            var settings = GetSettings();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var symbolDeclarations = assembly.GetCustomAttributes<DeclareScriptDefineSymbol>();

                if (symbolDeclarations == null)
                    continue;

                foreach (var symbolDeclaration in symbolDeclarations)
                {
                    ISymbol conflict = s_codeProvidedSymbols.Find(s => s.Name == symbolDeclaration.Name);
                    if (conflict != null)
                    {
                        Log.Warning($"A symbol with the name '{symbolDeclaration.Name}' has already been declared in the assembly '{conflict.CodeAssembly.GetName().Name}'. " +
                            $"Ignoring duplicate in assembly {assembly.GetName().Name}.");
                        continue;
                    }

                    conflict = settings.Symbols.Find(s => s.Name == symbolDeclaration.Name);
                    if (conflict != null)
                    {
                        Log.Warning($"A symbol with the name '{symbolDeclaration.Name}' has already been defined in the Project Settings. " +
                            $"Ignoring duplicate in assembly {assembly.GetName().Name}.");
                        continue;
                    }

                    var symbol = new CodeSymbole()
                    {
                        Name = symbolDeclaration.Name,
                        Description = symbolDeclaration.Description,
                        CodeAssembly = assembly
                    };
                    s_codeProvidedSymbols.Add(symbol);
                }
            }
        }

        public static ISymbol[] GetSymbols()
        {
            InitializeIfNeeded();

            ScriptDefineSymbolManagerSettings settings = GetSettings();

            List<ISymbol> combinedSymbols = new List<ISymbol>(settings.Symbols.Count + s_codeProvidedSymbols.Count);
            combinedSymbols.AddRange(settings.Symbols);
            combinedSymbols.AddRange(s_codeProvidedSymbols);

            return combinedSymbols.ToArray();
        }

        public static void CreateSymbol(string symbolName, string symbolDescription)
        {
            InitializeIfNeeded();
            ScriptDefineSymbolManagerSettings settings = GetSettings();
            settings.Symbols.AddUnique(new Symbol() { Name = symbolName, Description = symbolDescription });
            EditorUtility.SetDirty(settings);
        }

        public static void DeleteSymbol(string symbol)
        {
            InitializeIfNeeded();

            ScriptDefineSymbolManagerSettings settings = GetSettings();

            foreach (var p in GetProfiles())
            {
                RemoveSymbolFromProfile(symbol, p);
            }

            settings.Symbols.RemoveFirst((s) => s.Name == symbol);

            EditorUtility.SetDirty(settings);
        }

        public static void AddSymbolInProfile(string symbol, IProfile profile)
        {
            InitializeIfNeeded();
            ScriptDefineSymbolManagerSettings settings = GetSettings();
            if (settings.Symbols.Any((s) => s.Name == symbol) || s_codeProvidedSymbols.Any((s) => s.Name == symbol))
            {
                if (profile is BuildProfileWrapper p)
                {
                    var defines = p.Profile.scriptingDefines;
                    if (!defines.Contains(symbol))
                    {
                        defines = defines.Append(symbol).ToArray();
                        Array.Sort(defines);
                        p.Profile.scriptingDefines = defines;
                    }
                }
            }
            else
            {
                Debug.LogError($"Symbol {symbol} doesn't exist.");
            }

            EditorUtility.SetDirty(settings);
        }

        public static void EditSymbol(string symbolName, string newName, string newDescription)
        {
            InitializeIfNeeded();
            ScriptDefineSymbolManagerSettings settings = GetSettings();

            Symbol symbol = settings.Symbols.Find((s) => s.Name == symbolName);

            if (symbol == null)
            {
                Log.Error($"No user-created symbol named {symbolName} exists.");
                return;
            }

            symbol.Name = newName;
            symbol.Description = newDescription;

            foreach (var profile in GetProfiles())
            {
                if (RemoveSymbolFromProfile(symbolName, profile))
                {
                    AddSymbolInProfile(symbolName, profile);
                }
            }

            EditorUtility.SetDirty(settings);
        }

        public static bool RemoveSymbolFromProfile(string symbol, IProfile profile)
        {
            if (profile is BuildProfileWrapper p && p.Profile.scriptingDefines.Contains(symbol))
            {
                p.Profile.scriptingDefines = p.Profile.scriptingDefines.Where(x => x != symbol).ToArray();
                EditorUtility.SetDirty(p.Profile);
                return true;
            }

            return false;
        }

        public static IProfile GetProfile(string name)
        {
            return GetProfiles().Find((p) => p.Name == name);
        }

        private static ScriptDefineSymbolManagerSettings GetSettings()
        {
            if (s_settings == null)
            {
                s_settings = ScriptDefineSymbolManagerSettings.GetOrCreateSettings();
            }

            return s_settings;
        }
    }

    public static class ScriptDefineSymbolManagerProfileExtensions
    {
        public static string GetCombinedSymbols(this ScriptDefineSymbolManager.IProfile profile) => string.Join(";", profile.DefinedSymbols);
    }
}