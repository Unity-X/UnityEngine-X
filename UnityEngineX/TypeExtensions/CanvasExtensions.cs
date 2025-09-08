using UnityEngine;

namespace UnityEngineX
{
    public static class CanvasExtensions
    {
        public static Vector2 WorldToCanvasPosition(this Canvas canvas, Camera camera, Vector3 position)
        {
            var canvasRectTransform = canvas.transform as RectTransform;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
            return ScreenToCanvasPosition(canvas, screenPoint);
        }

        public static Vector2 ScreenToCanvasPosition(this Canvas canvas, Vector2 screenPoint)
        {
            var canvasRectTransform = canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 result);

            return canvasRectTransform.TransformPoint(result);
        }
    }
}