using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngineX;

namespace UnityEditorX
{
    public class ScriptDefineSymbolManagerSettings : ScriptableObject
    {
        [System.Serializable]
        internal class Profile : ScriptDefineSymbolManager.IProfile
        {
            public string Name;
            public List<string> Symbols = new List<string>();

            string ScriptDefineSymbolManager.IProfile.Name => Name;
            ReadOnlyList<string> ScriptDefineSymbolManager.IProfile.DefinedSymbols => Symbols.AsReadOnlyNoAlloc();
        }

        [System.Serializable]
        internal class Symbol : ScriptDefineSymbolManager.ISymbol
        {
            public string Name;
            public string Description;

            string ScriptDefineSymbolManager.ISymbol.Name => Name;
            string ScriptDefineSymbolManager.ISymbol.Description => Description;
            bool ScriptDefineSymbolManager.ISymbol.ProvidedByCode => false;
            Assembly ScriptDefineSymbolManager.ISymbol.CodeAssembly => null;
        }

        [SerializeField] internal List<Profile> Profiles = new List<Profile>();
        [SerializeField] internal List<Symbol> Symbols = new List<Symbol>();


        #region Asset management
        private const string ASSET_PATH = "Assets/Config/ScriptDefineSymbolManagerSettings.asset";

        internal static ScriptDefineSymbolManagerSettings GetOrCreateSettings()
        {
            return AssetDatabaseX.LoadOrCreateScriptableObjectAsset<ScriptDefineSymbolManagerSettings>(ASSET_PATH);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        #endregion
    }
}