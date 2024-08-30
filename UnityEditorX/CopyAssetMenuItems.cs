using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityEditorX
{
    public static class CopyAssetMenuItems
    {
        [MenuItem(itemName: "Assets/Copy/GUID", priority = 19)]
        public static void CopyGUID()
        {
            List<string> str = new();
            foreach (var item in Selection.assetGUIDs)
            {
                str.Add(item);
            }

            str.Sort();
            SetClipboard(string.Join("\n", str));
        }

        [MenuItem(itemName: "Assets/Copy/GUID", validate = true)]
        public static bool Validate_CopyGUID() => Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0;


        [MenuItem(itemName: "Assets/Copy/Full Path", priority = 19)]
        public static void CopyFullPath()
        {
            List<string> str = new();
            foreach (var item in Selection.assetGUIDs)
            {
                str.Add(AssetDatabase.GUIDToAssetPath(item));
            }
            str.Sort();
            SetClipboard(string.Join("\n", str));
        }

        [MenuItem(itemName: "Assets/Copy/Full Path", validate = true)]
        public static bool Validate_CopyFullPath() => Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0;


        [MenuItem(itemName: "Assets/Copy/Project Relative Path", priority = 19)]
        public static void CopyProjectRelativePath()
        {
            List<string> str = new();
            foreach (var item in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(item);
                char unwantedSeparator = (Path.DirectorySeparatorChar == '/') ? '\\' : '/';
                assetPath = assetPath.Replace(unwantedSeparator, Path.DirectorySeparatorChar);
                str.Add(assetPath);
            }

            str.Sort();
            SetClipboard(string.Join("\n", str));
        }

        [MenuItem(itemName: "Assets/Copy/Project Relative Path", validate = true)]
        public static bool Validate_CopyProjectRelativePath() => Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0;

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