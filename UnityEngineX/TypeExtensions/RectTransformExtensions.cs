using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngineX
{
    public static class RectTransformExtensions
    {
        public static Rect GetRectRelativeTo(this RectTransform targetRectTransform, RectTransform relativeRectTransform)
        {
            Rect aRect = targetRectTransform.rect;
            Rect bRect = relativeRectTransform.rect;

            Vector2 relativeSize = relativeRectTransform.InverseTransformVector(targetRectTransform.TransformVector(aRect.size));
            Vector2 relativeMin = relativeRectTransform.InverseTransformPoint(targetRectTransform.TransformPoint(aRect.min));

            return new Rect(relativeMin - bRect.min, relativeSize);
        }

        public static void SetScreenPosition(this RectTransform rectTransform, Vector2 screenPoint)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();

            if (canvas == null)
                throw new Exception("No canvas found in parents.");
            Vector2 result = canvas.ScreenToCanvasPosition(screenPoint);
            rectTransform.position = result;
        }

        public static Rect GetScreenRect(this RectTransform rectTr)
        {
            var canvas = rectTr.GetComponentInParent<Canvas>();

            if (canvas == null)
                throw new Exception("No canvas found in parents.");

            canvas = canvas.rootCanvas;

            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                Rect rect = rectTr.rect;

                Camera camera = canvas.worldCamera;
                if (camera == null)
                    throw new Exception("No camera attached to canvas with 'WorldSpace' render mode.");

                Vector2 screenMin = camera.WorldToScreenPoint(rectTr.TransformPoint(rect.min));
                Vector2 screenMax = camera.WorldToScreenPoint(rectTr.TransformPoint(rect.max));

                return new Rect(screenMin, screenMax - screenMin);
            }
            else
            {
                Rect rect = rectTr.GetRectRelativeTo((RectTransform)canvas.transform);

                rect.position *= canvas.scaleFactor;
                rect.size *= canvas.scaleFactor;

                return rect;
            }
        }
    }
}