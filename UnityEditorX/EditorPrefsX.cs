using System;
using System.Collections.ObjectModel;
using UnityEditor;

public static class EditorPrefsX
{
    public static readonly ReadOnlyCollection<Type> SupportedValueTypes = Array.AsReadOnly(new Type[]
    {
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(bool),
    });

    public static void Set(string key, object v)
    {
        switch (v)
        {
            case short s:
                SetShort(key, s);
                break;
            case ushort us:
                SetUShort(key, us);
                break;
            case int i:
                EditorPrefs.SetInt(key, i);
                break;
            case uint ui:
                SetUInt(key, ui);
                break;
            case long l:
                SetLong(key, l);
                break;
            case ulong ul:
                SetULong(key, ul);
                break;
            case double d:
                SetDouble(key, d);
                break;
            case float f:
                EditorPrefs.SetFloat(key, f);
                break;
            case string s:
                EditorPrefs.SetString(key, s);
                break;
            case bool b:
                SetBool(key, b);
                break;
            default:
                throw new System.Exception("Unsupported value type. Supported types are short, ushort, int, uint, long, ulong, double, float, string and bool");
        }
    }

    public static bool TryGet<T>(string key, object defaultValue, out object value)
    {
        return TryGet(key, typeof(T), defaultValue, out value);
    }

    public static bool TryGet(string key, Type valueType, object defaultValue, out object value)
    {
        bool res = true;
        value = null;

        if (valueType == typeof(short))
        {
            value = GetShort(key, (short)(defaultValue ?? default(short)));
        }
        else if (valueType == typeof(ushort))
        {
            value = GetUShort(key, (ushort)(defaultValue ?? default(ushort)));
        }
        else if (valueType == typeof(int))
        {
            value = EditorPrefs.GetInt(key, (int)(defaultValue ?? default(int)));
        }
        else if (valueType == typeof(uint))
        {
            res = TryGetUInt(key, (uint)(defaultValue ?? default(uint)), out uint v);
            value = v;
        }
        else if (valueType == typeof(long))
        {
            res = TryGetLong(key, (long)(defaultValue ?? default(long)), out long v);
            value = v;
        }
        else if (valueType == typeof(ulong))
        {
            res = TryGetULong(key, (ulong)(defaultValue ?? default(ulong)), out ulong v);
            value = v;
        }
        else if (valueType == typeof(float))
        {
            value = EditorPrefs.GetFloat(key, (float)(defaultValue ?? default(float)));
        }
        else if (valueType == typeof(double))
        {
            res = TryGetDouble(key, (double)(defaultValue ?? default(double)), out double v);
            value = v;
        }
        else if (valueType == typeof(string))
        {
            value = EditorPrefs.GetString(key, defaultValue == null ? null : (string)defaultValue);
        }
        else if (valueType == typeof(bool))
        {
            value = GetBool(key, (bool)(defaultValue ?? default(bool)));
        }
        else
        {
            throw new System.Exception("Unsupported value type. Supported types are short, ushort, int, uint, long, ulong, double, float, string and bool");
        }
        return res;
    }

    public static void SetShort(string key, short v) => EditorPrefs.SetInt(key, v);
    public static void SetUShort(string key, ushort v) => EditorPrefs.SetInt(key, v);
    public static void SetUInt(string key, uint v) => EditorPrefs.SetString(key, v.ToString("R"));
    public static void SetLong(string key, long v) => EditorPrefs.SetString(key, v.ToString("R"));
    public static void SetULong(string key, ulong v) => EditorPrefs.SetString(key, v.ToString("R"));
    public static void SetDouble(string key, double v) => EditorPrefs.SetString(key, v.ToString("G17"));
    public static void SetBool(string key, bool v) => EditorPrefs.SetInt(key, v ? 1 : 0);

    public static short GetShort(string key, short defaultValue) => (short)EditorPrefs.GetInt(key, defaultValue);
    public static ushort GetUShort(string key, ushort defaultValue) => (ushort)EditorPrefs.GetInt(key, defaultValue);
    public static uint GetUInt(string key, uint defaultValue) => uint.Parse(EditorPrefs.GetString(key, defaultValue.ToString("R")));
    public static long GetLong(string key, long defaultValue) => long.Parse(EditorPrefs.GetString(key, defaultValue.ToString("R")));
    public static ulong GetULong(string key, ulong defaultValue) => ulong.Parse(EditorPrefs.GetString(key, defaultValue.ToString("R")));
    public static double GetDouble(string key, double defaultValue) => double.Parse(EditorPrefs.GetString(key, defaultValue.ToString("G17")));
    public static bool GetBool(string key, bool defaultValue) => EditorPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

    public static bool TryGetUInt(string key, uint defaultValue, out uint value) => uint.TryParse(EditorPrefs.GetString(key, defaultValue.ToString("R")), out value);
    public static bool TryGetLong(string key, long defaultValue, out long value) => long.TryParse(EditorPrefs.GetString(key, defaultValue.ToString("R")), out value);
    public static bool TryGetULong(string key, ulong defaultValue, out ulong value) => ulong.TryParse(EditorPrefs.GetString(key, defaultValue.ToString("R")), out value);
    public static bool TryGetDouble(string key, double defaultValue, out double value) => double.TryParse(EditorPrefs.GetString(key, defaultValue.ToString("G17")), out value);
}
