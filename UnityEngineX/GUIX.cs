using UnityEngine;


namespace UnityEngineX
{
    public static class GUIX
    {
        private static Texture2D s_quad;

        public static void DrawQuad(Rect position, Color color)
        {
            if (s_quad == null)
            {
                s_quad = new Texture2D(1, 1);
                s_quad.SetPixel(0, 0, color);
                s_quad.Apply();
            }

            var wasBg = GUI.skin.box.normal.background;
            GUI.skin.box.normal.background = s_quad;
            GUI.Box(position, GUIContent.none);
            GUI.skin.box.normal.background = wasBg;
        }
    }
}