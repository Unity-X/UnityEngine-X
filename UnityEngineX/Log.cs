﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngineX;

[assembly: DeclareScriptDefineSymbol("UNITY_X_LOG_ASSERT", "Enables asserts with Log.Assert(..). If disabled, all calls to these methods are stripped from the build.")]
[assembly: DeclareScriptDefineSymbol("UNITY_X_LOG_ERROR", "Enables errors logs with Log.Error(..). If disabled, all calls to these methods are stripped from the build.")]
[assembly: DeclareScriptDefineSymbol("UNITY_X_LOG_INFO", "Enables info logs with Log.Info(..). If disabled, all calls to these methods are stripped from the build.")]
[assembly: DeclareScriptDefineSymbol("UNITY_X_LOG_EXCEPTION", "Enables exception logs with Log.Exception(..). If disabled, all calls to these methods are stripped from the build.")]
[assembly: DeclareScriptDefineSymbol("UNITY_X_LOG_WARNING", "Enables warning logs with Log.Warning(..). If disabled, all calls to these methods are stripped from the build.")]

namespace UnityEngineX
{
    public class LogChannel
    {
        internal LogChannel() { }

        public string Name { get; internal set; }
        public int Id { get; internal set; }
        public bool Active { get; internal set; }

        internal bool ActiveByDefault { get; set; }
    }

    public static class Log
    {
        public static class Internals
        {
            public delegate void LogCallback(int channelId, string condition, string stackTrace, LogType logType);

            public static void ForceInitialize() => Log.Initialize();

            public static event LogCallback LogMessageReceived
            {
                add => Log.LogMessageReceived += value;
                remove => Log.LogMessageReceived -= value;
            }
            public static event LogCallback LogMessageReceivedThreaded
            {
                add => Log.LogMessageReceivedThreaded += value;
                remove => Log.LogMessageReceivedThreaded -= value;
            }
        }
        internal static class ChannelManager
        {
            private static ReaderWriterLockSlim s_channelsLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            private static List<LogChannel> s_channels = new List<LogChannel>();

            public static LogChannel[] GetChannels()
            {
                s_channelsLock.EnterReadLock();
                var result = s_channels.ToArray();
                s_channelsLock.ExitReadLock();
                return result;
            }

            internal static LogChannel CreateChannel(string name, bool activeByDefault = true)
            {
                s_channelsLock.EnterWriteLock();

                LogChannel newChannel;

                if (s_channels.Any(c => c.Name == name))
                {
                    Log.Error($"Log channel with named {name} already exists.");
                    newChannel = null;
                }
                else
                {
                    newChannel = new LogChannel()
                    {
                        Name = name,
                        Id = s_channels.Count,
                        ActiveByDefault = activeByDefault,
                        Active = IsActiveInSettings(name, activeByDefault)
                    };

                    s_channels.Add(newChannel);
                }

                s_channelsLock.ExitWriteLock();

                return newChannel;
            }

            internal static void SetChannelActive(LogChannel channel, bool value)
            {
                if (channel is null)
                {
                    throw new ArgumentNullException(nameof(channel));
                }

                channel.Active = value;
                SetActiveInSettings(channel.Name, value);
            }

            internal static LogChannel GetChannel(int id)
            {
                LogChannel foundChannel = null;

                if (id >= 0 && id < s_channels.Count)
                {
                    s_channelsLock.EnterReadLock();
                    foundChannel = s_channels[id];
                    s_channelsLock.ExitReadLock();
                }

                return foundChannel;
            }

            internal static void Initialize()
            {
                s_channelsLock.EnterReadLock();
                foreach (var item in s_channels)
                {
                    item.Active = IsActiveInSettings(item.Name, item.ActiveByDefault);
                }
                s_channelsLock.ExitReadLock();
            }

            private const string KEY_PREFIX = "unity-x-log-channel-";

            private static bool IsActiveInSettings(string name, bool activeByDefault)
            {
                if (s_initialized)
                    return PlayerPrefs.GetInt(KEY_PREFIX + name, defaultValue: activeByDefault ? 1 : 0) == 1;
                else
                    return true;
            }
            private static void SetActiveInSettings(string name, bool active)
            {
                PlayerPrefs.SetInt(KEY_PREFIX + name, active ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static bool s_initialized = false;

        private static event Internals.LogCallback LogMessageReceived;
        private static event Internals.LogCallback LogMessageReceivedThreaded;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod] // initializes in editor
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] // initializes in build & playmode
        private static void Initialize()
        {
            if (s_initialized)
                return;
            s_initialized = true;

            Application.logMessageReceived += OnMessageReceived_DebugLog;
            Application.logMessageReceivedThreaded += OnMessageReceived_DebugLog_Threaded;

            Initialize();
        }

        private static void OnMessageReceived_DebugLog(string condition, string stackTrace, LogType type)
        {
            PopChannelForCurrentThread(out int channelId);
            LogMessageReceived?.Invoke(channelId, condition, stackTrace, type);
        }

        private static void OnMessageReceived_DebugLog_Threaded(string condition, string stackTrace, LogType type)
        {
            PopChannelForCurrentThread(out int channelId);
            LogMessageReceivedThreaded?.Invoke(channelId, condition, stackTrace, type);
        }

        private static ConcurrentDictionary<int, int> s_threadIdToChannelId = new ConcurrentDictionary<int, int>();

        private static bool TryUseChannel(int channelId)
        {
            LogChannel channel = ChannelManager.GetChannel(channelId);
            if (channel == null)
            {
                Debug.LogError($"No channel with the id '{channelId}' exists.");
                return false;
            }

            // check if channel is active
            if (!channel.Active)
            {
                return false;
            }

            // set channel to use for current thread
            s_threadIdToChannelId.TryAdd(Thread.CurrentThread.ManagedThreadId, channelId);

            return true;
        }

        private static void PopChannelForCurrentThread(out int channelId)
        {
            if (!s_threadIdToChannelId.TryRemove(Thread.CurrentThread.ManagedThreadId, out channelId))
            {
                channelId = -1;
            }
        }

        public static LogChannel CreateChannel(string name, bool activeByDefault = true) => ChannelManager.CreateChannel(name, activeByDefault);
        public static void SetChannelActive(LogChannel channel, bool value) => ChannelManager.SetChannelActive(channel, value);
        public static LogChannel[] GetChannels() => ChannelManager.GetChannels();
        public static LogChannel GetChannel(int id) => ChannelManager.GetChannel(id);

        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition, string message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition, message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition, object message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition, message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition, string message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition, message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition, object message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition, message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(int channelId, bool condition, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.Assert(condition, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(int channelId, bool condition, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.AssertFormat(condition, context, format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(int channelId, bool condition, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.AssertFormat(condition, format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition, string message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition, message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition, object message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition, message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition, string message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition, message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition, object message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition, message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(LogChannel channel, bool condition, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.Assert(condition, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(LogChannel channel, bool condition, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.AssertFormat(condition, context, format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(LogChannel channel, bool condition, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.AssertFormat(condition, format, args);
#endif
        }

        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition, string message, UnityEngine.Object context) { Debug.Assert(condition, message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition) { Debug.Assert(condition); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition, object message, UnityEngine.Object context) { Debug.Assert(condition, message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition, string message) { Debug.Assert(condition, message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition, object message) { Debug.Assert(condition, message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assert(bool condition, UnityEngine.Object context) { Debug.Assert(condition, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(bool condition, UnityEngine.Object context, string format, params object[] args) { Debug.AssertFormat(condition, context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertFormat(bool condition, string format, params object[] args) { Debug.AssertFormat(condition, format, args); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(int channelId, object message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.LogAssertion(message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(int channelId, object message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.LogAssertion(message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(int channelId, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.LogAssertionFormat(context, format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(int channelId, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channelId))
                Debug.LogAssertionFormat(format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(LogChannel channel, object message)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.LogAssertion(message);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(LogChannel channel, object message, UnityEngine.Object context)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.LogAssertion(message, context);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(LogChannel channel, UnityEngine.Object context, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.LogAssertionFormat(context, format, args);
#endif
        }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(LogChannel channel, string format, params object[] args)
        {
#if UNITY_ASSERTIONS
            if (TryUseChannel(channel.Id))
                Debug.LogAssertionFormat(format, args);
#endif
        }

        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(object message) { Debug.LogAssertion(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void Assertion(object message, UnityEngine.Object context) { Debug.LogAssertion(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(UnityEngine.Object context, string format, params object[] args) { Debug.LogAssertionFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ASSERT")]
        public static void AssertionFormat(string format, params object[] args) { Debug.LogAssertionFormat(format, args); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(int channelId, object message) { if (TryUseChannel(channelId)) Debug.LogError(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(int channelId, object message, UnityEngine.Object context) { if (TryUseChannel(channelId)) Debug.LogError(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(LogChannel channel, object message) { if (TryUseChannel(channel.Id)) Debug.LogError(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(LogChannel channel, object message, UnityEngine.Object context) { if (TryUseChannel(channel.Id)) Debug.LogError(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(object message) { Debug.LogError(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void Error(object message, UnityEngine.Object context) { Debug.LogError(message, context); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(int channelId, UnityEngine.Object context, string format, params object[] args) { if (TryUseChannel(channelId)) Debug.LogErrorFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(int channelId, string format, params object[] args) { if (TryUseChannel(channelId)) Debug.LogErrorFormat(format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(LogChannel channel, UnityEngine.Object context, string format, params object[] args) { if (TryUseChannel(channel.Id)) Debug.LogErrorFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(LogChannel channel, string format, params object[] args) { if (TryUseChannel(channel.Id)) Debug.LogErrorFormat(format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(UnityEngine.Object context, string format, params object[] args) { Debug.LogErrorFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_ERROR")]
        public static void ErrorFormat(string format, params object[] args) { Debug.LogErrorFormat(format, args); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(int channelId, Exception exception) { if (TryUseChannel(channelId)) Debug.LogException(exception); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(int channelId, Exception exception, UnityEngine.Object context) { if (TryUseChannel(channelId)) Debug.LogException(exception, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(LogChannel channel, Exception exception) { if (TryUseChannel(channel.Id)) Debug.LogException(exception); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(LogChannel channel, Exception exception, UnityEngine.Object context) { if (TryUseChannel(channel.Id)) Debug.LogException(exception, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(Exception exception) { Debug.LogException(exception); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_EXCEPTION")]
        public static void Exception(Exception exception, UnityEngine.Object context) { Debug.LogException(exception, context); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(int channelId, object message) { if (TryUseChannel(channelId)) Debug.Log(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(int channelId, object message, UnityEngine.Object context) { if (TryUseChannel(channelId)) Debug.Log(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(LogChannel channel, object message) { if (TryUseChannel(channel.Id)) Debug.Log(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(LogChannel channel, object message, UnityEngine.Object context) { if (TryUseChannel(channel.Id)) Debug.Log(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(object message) { Debug.Log(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_INFO")]
        public static void Info(object message, UnityEngine.Object context) { Debug.Log(message, context); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(int channelId, object message) { if (TryUseChannel(channelId)) Debug.LogWarning(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(int channelId, object message, UnityEngine.Object context) { if (TryUseChannel(channelId)) Debug.LogWarning(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(LogChannel channel, object message) { if (TryUseChannel(channel.Id)) Debug.LogWarning(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(LogChannel channel, object message, UnityEngine.Object context) { if (TryUseChannel(channel.Id)) Debug.LogWarning(message, context); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(object message) { Debug.LogWarning(message); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void Warning(object message, UnityEngine.Object context) { Debug.LogWarning(message, context); }

        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(int channelId, UnityEngine.Object context, string format, params object[] args) { if (TryUseChannel(channelId)) Debug.LogWarningFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(int channelId, string format, params object[] args) { if (TryUseChannel(channelId)) Debug.LogWarningFormat(format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(LogChannel channel, UnityEngine.Object context, string format, params object[] args) { if (TryUseChannel(channel.Id)) Debug.LogWarningFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(LogChannel channel, string format, params object[] args) { if (TryUseChannel(channel.Id)) Debug.LogWarningFormat(format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(UnityEngine.Object context, string format, params object[] args) { Debug.LogWarningFormat(context, format, args); }
        [System.Diagnostics.Conditional("UNITY_X_LOG_WARNING")]
        public static void WarningFormat(string format, params object[] args) { Debug.LogWarningFormat(format, args); }
    }
}