using System;
using System.Collections.Generic;
using System.Linq;

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


        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
                              .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                              .Where(arg => !string.IsNullOrEmpty(arg));
        }

        private static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}
