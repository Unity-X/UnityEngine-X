using System;
using UnityEngine;

namespace UnityEngineX
{
    public static class ActionExtensions
    {
        static public void InvokeCatchException<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2, LogMode logMode = LogMode.Error)
        {
            try
            {
                action.Invoke(t1, t2);
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
        }
        static public void InvokeCatchException<T>(this Action<T> action, T t, LogMode logMode = LogMode.Error)
        {
            try
            {
                action.Invoke(t);
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
        }
        static public void InvokeCatchException(this Action action, LogMode logMode = LogMode.Error)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
        }
        static public void InvokeCatchExceptionInEditor<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2, LogMode logMode = LogMode.Error)
        {
#if UNITY_EDITOR
            try
            {
#endif
                action.Invoke(t1, t2);
#if UNITY_EDITOR
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
#endif
        }
        static public void InvokeCatchExceptionInEditor<T>(this Action<T> action, T t, LogMode logMode = LogMode.Error)
        {
#if UNITY_EDITOR
            try
            {
#endif
                action.Invoke(t);
#if UNITY_EDITOR
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
#endif
        }
        static public void InvokeCatchExceptionInEditor(this Action action, LogMode logMode = LogMode.Error)
        {
#if UNITY_EDITOR
            try
            {
#endif
                action.Invoke();
#if UNITY_EDITOR
            }
            catch (Exception e)
            {
                Log(e, logMode);
            }
#endif
        }

        static private void Log(Exception exception, LogMode logMode)
        {
            string message = $"Error in SafeInvoke: {exception.Message}  \nStack:" + exception.StackTrace;

            switch (logMode)
            {
                case LogMode.None:
                    break;

                case LogMode.Info:
                    Debug.Log(message);
                    break;

                case LogMode.Warning:
                    Debug.LogWarning(message);
                    break;

                case LogMode.Error:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}