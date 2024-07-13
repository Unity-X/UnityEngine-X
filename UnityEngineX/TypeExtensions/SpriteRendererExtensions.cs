using UnityEngine;

namespace UnityEngineX
{
    public static class SpriteRendererExtensions
    {
        public static void SetAlpha(this SpriteRenderer sprR, float alpha)
        {
            Color c = sprR.color;
            sprR.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}