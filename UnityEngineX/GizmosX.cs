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
#endif
        }

        public static void DrawHexagon(Vector2 position, float radius, float rotation)
        {
            Vector3[] points = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                points[i] = position + Rotate(new Vector2(radius, 0), 60 * i + rotation);
            }

            Gizmos.DrawLineStrip(points, looped: true);

            Vector2 Rotate(Vector2 v, float angle)
            {
                return RotateRad(v, angle * Mathf.Deg2Rad);
            }
            Vector2 RotateRad(Vector2 v, float radians)
            {
                float sin = Mathf.Sin(radians);
                float cos = Mathf.Cos(radians);

                float tx = v.x;
                float ty = v.y;

                return new Vector2(
                    (cos * tx) - (sin * ty),    // x
                    (sin * tx) + (cos * ty));   // y
            }
        }


        public static void DrawCircle(Vector3 position, Color color, float radius = 1.0f)
        {
            DrawCircle(position, Vector3.up, color, radius);
        }

        public static void DrawCircle(Vector3 position, Vector3 up, float radius = 1.0f)
        {
            DrawCircle(position, position, Color.white, radius);
        }

        public static void DrawCircle(Vector3 position, float radius = 1.0f)
        {
            DrawCircle(position, Vector3.up, Color.white, radius);
        }

        public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f)
        {
            up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 _right = Vector3.Cross(up, _forward).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;

            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 _nextPoint = Vector3.zero;

            Color oldColor = Gizmos.color;
            Gizmos.color = (color == default(Color)) ? Color.white : color;

            for (var i = 0; i < 91; i++)
            {
                _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
                _nextPoint.y = 0;

                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

                Gizmos.DrawLine(_lastPoint, _nextPoint);
                _lastPoint = _nextPoint;
            }

            Gizmos.color = oldColor;
        }

        public static void DrawCone(Vector3 position, Vector3 direction, Color color, float angle = 45)
        {
            float length = direction.magnitude;

            Vector3 _forward = direction;
            Vector3 _up = Vector3.Slerp(_forward, -_forward, 0.5f);
            Vector3 _right = Vector3.Cross(_forward, _up).normalized * length;

            direction = direction.normalized;

            Vector3 slerpedVector = Vector3.Slerp(_forward, _up, angle / 90.0f);

            float dist;
            var farPlane = new Plane(-direction, position + _forward);
            var distRay = new Ray(position, slerpedVector);

            farPlane.Raycast(distRay, out dist);

            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            Gizmos.DrawRay(position, slerpedVector.normalized * dist);
            Gizmos.DrawRay(position, Vector3.Slerp(_forward, -_up, angle / 90.0f).normalized * dist);
            Gizmos.DrawRay(position, Vector3.Slerp(_forward, _right, angle / 90.0f).normalized * dist);
            Gizmos.DrawRay(position, Vector3.Slerp(_forward, -_right, angle / 90.0f).normalized * dist);

            DrawCircle(position + _forward, direction, color, (_forward - (slerpedVector.normalized * dist)).magnitude);
            DrawCircle(position + (_forward * 0.5f), direction, color, ((_forward * 0.5f) - (slerpedVector.normalized * (dist * 0.5f))).magnitude);

            Gizmos.color = oldColor;
        }

        public static void DrawCone(Vector3 position, Vector3 direction, float angle = 45)
        {
            DrawCone(position, direction, Color.white, angle);
        }

        public static void DrawCone(Vector3 position, Color color, float angle = 45)
        {
            DrawCone(position, Vector3.up, color, angle);
        }

        public static void DrawCone(Vector3 position, float angle = 45)
        {
            DrawCone(position, Vector3.up, Color.white, angle);
        }

        public static void DrawArrow(Vector3 from, Vector3 to, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            Vector3 direction = (to - from);
            Gizmos.DrawRay(from, direction);
            DrawCone(from + direction, -direction * 0.333f, color, 15);

            Gizmos.color = oldColor;
        }

        public static void DrawArrow(Vector3 from, Vector3 to)
        {
            DrawArrow(from, to, Color.white);
        }
    }
}