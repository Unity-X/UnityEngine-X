using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditorX
{
    public static class CopyAssetMenuItems
    {
        [MenuItem(itemName: "Assets/Copy/GUID", priority = 19)]
        public static void CopyGUID()
        {
            string assetPath = AssetDatabase.GetAssetPath(GetCurrentSingleSelection());
            string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);

            SetClipboard(assetGUID);
        }

        [MenuItem(itemName: "Assets/Copy/GUID", validate = true)]
        public static bool Validate_CopyGUID() => IsCurrentSelectionValid();


        [MenuItem(itemName: "Assets/Copy/Full Path", priority = 19)]
        public static void CopyFullPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(GetCurrentSingleSelection());
            string fullPath = Path.GetFullPath(assetPath);

            SetClipboard(fullPath);
        }

        [MenuItem(itemName: "Assets/Copy/Full Path", validate = true)]
        public static bool Validate_CopyFullPath() => IsCurrentSelectionValid();


        [MenuItem(itemName: "Assets/Copy/Project Relative Path", priority = 19)]
        public static void CopyProjectRelativePath()
        {
            string assetPath = AssetDatabase.GetAssetPath(GetCurrentSingleSelection());

            char unwantedSeparator = (Path.DirectorySeparatorChar == '/') ? '\\' : '/';

            assetPath = assetPath.Replace(unwantedSeparator, Path.DirectorySeparatorChar);

            SetClipboard(assetPath);
        }

        [MenuItem(itemName: "Assets/Copy/Project Relative Path", validate = true)]
        public static bool Validate_CopyProjectRelativePath() => IsCurrentSelectionValid();

        private static Object GetCurrentSingleSelection()
        {
            if (Selection.objects != null && Selection.objects.Length == 1)
                return Selection.objects[0];
            return null;
        }

        private static bool IsCurrentSelectionValid()
        {
            var selection = GetCurrentSingleSelection();
            return selection != null && AssetDatabase.IsMainAsset(selection);
        }

        private static void SetClipboard(string value)
        {
            var textEditor = new TextEditor
            {
                text = value
            };

            textEditor.OnFocus();
            textEditor.Copy();
        }
    }
}