using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditorX
{
    public class ScriptDefineSymbolManagerSettings : ScriptableObject
    {
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