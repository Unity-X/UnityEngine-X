using System;
using System.Collections.Generic;

namespace UnityEngineX
{
    public static class CommandLine
    {
        static List<string> s_arguments;

        public static ReadOnlyList<string> Arguments
        {
            get
            {
                if (s_arguments == null)
                    s_arguments = new List<string>(Environment.GetCommandLineArgs());
                return s_arguments.AsReadOnlyNoAlloc();
            }
        }

        public static string CompleteCommandLine => Environment.CommandLine;

        public static bool TryGetInt(string key, out int value)
        {
            if (TryGetArgumentMatchedWithValue(key, out int index))
            {
                return int.TryParse(Arguments[index + 1], out value);
            }
            value = default;
            return false;
        }

        public static bool TryGetFloat(string key, out float value)
        {
            if (TryGetArgumentMatchedWithValue(key, out int index))
            {
                return float.TryParse(Arguments[index + 1], out value);
            }
            value = default;
            return false;
        }

        public static bool TryGetString(string key, out string value)
        {
            if (TryGetArgumentMatchedWithValue(key, out int index))
            {
                value = Arguments[index + 1];
                return true;
            }
            value = default;
            return false;
        }

        static bool TryGetArgumentMatchedWithValue(string key, out int index)
        {
            index = Arguments.IndexOf(key);
            if (index != -1 && index < Arguments.Count - 1)
            {
                return true;
            }
            return false;
        }
    }
}
