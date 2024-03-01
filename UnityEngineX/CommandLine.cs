using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngineX
{
    public static class CommandLine
    {
        static List<string> s_arguments;
        static List<string> s_argumentGrouped;


        /// <summary>
        /// Eg.
        /// <list type="number">
        /// <item>-resolution</item>
        /// <item>1920</item>
        /// <item>1080</item>
        /// <item>-map</item>
        /// <item>Tundra</item>
        /// </list>
        /// </summary>
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

        /// <summary>
        /// This will split the comment line into arguments by looking for spaces (spaces inside " " or { } are ignored).
        /// </summary>
        public static IEnumerable<string> SplitCommandLineInGroups(string commandLine)
        {
            bool inQuotes = false;

            int inGroupDepth = 0;

            return commandLine.Split(c =>
            {
                if (c == '{')
                    inGroupDepth++;
                else if (c == '}')
                    inGroupDepth--;
                else if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && inGroupDepth == 0 && c == ' ';
            })
                              .Select(arg => arg.Trim().RemoveDelimiters('\"', '\"').RemoveDelimiters('{', '}'))
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

        private static string RemoveDelimiters(this string input, char first, char last)
        {
            if ((input.Length >= 2) &&
                (input[0] == first) && (input[input.Length - 1] == last))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}
