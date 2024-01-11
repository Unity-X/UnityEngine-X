using UnityEngine;


namespace UnityEngineX
{
    public static class GizmosX
    {
        private static GUIStyle s_stringLabelStyle;

        public static void DrawString(string text, Vector3 worldPos, Color? textColor = null, Color? backColor = null)
        {
#if UNITY_EDITOR
            if (s_stringLabelStyle == null)
            {
                s_stringLabelStyle = new(UnityEditor.EditorStyles.label);
                s_stringLabelStyle.alignment = TextAnchor.MiddleCenter;
                s_stringLabelStyle.border = new();
                s_stringLabelStyle.margin = new();
                s_stringLabelStyle.padding = new(2, 2, 0, 0);
                s_stringLabelStyle.normal.textColor = Color.white;
                s_stringLabelStyle.hover.textColor = s_stringLabelStyle.normal.textColor;
            }

            var restoreTextColor = GUI.color;
            var restoreBackColor = GUI.backgroundColor;

            UnityEditor.Handles.BeginGUI();

            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (view != null && view.camera != null)
            {
                Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
                if (screenPos.y >= 0 && screenPos.y <= Screen.height && screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.z >= 0)
                {
                    Vector2 size = s_stringLabelStyle.CalcSize(new GUIContent(text));
                    var r = new Rect(screenPos.x - (size.x / 2), view.cameraViewport.height - screenPos.y - (size.y / 2), size.x, size.y);

                    GUIX.DrawQuad(r, backColor ?? Color.black);
                    GUI.color = textColor ?? Color.white;
                    GUI.Label(r, text, s_stringLabelStyle);
                }
            }
            UnityEditor.Handles.EndGUI();

            GUI.color = restoreTextColor;
            GUI.backgroundColor = restoreBackColor;
        }
#endif
    }
}