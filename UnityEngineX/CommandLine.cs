using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UnityEngineX
{
    public static class CommandLine
    {
        static List<List<string>> s_argumentParams = new();
        static List<string> s_argumentsGrouped;
        static List<string> s_argumentKeys = new();
        static bool s_init = false;

        private static void Init()
        {
            if (s_init)
                return;
            s_init = true;
            InitialiseGroupedArguments();
        }


        /// <summary>
        /// Eg.
        /// <list type="number">
        /// <item>-resolution 1920 1080</item>
        /// <item>-map Tundra</item>
        /// </list>
        /// </summary>
        public static ReadOnlyList<string> GroupedArguments
        {
            get
            {
                Init();
                return s_argumentsGrouped.AsReadOnlyNoAlloc();
            }
        }

        private static void InitialiseGroupedArguments()
        {
            string line = Environment.CommandLine;

            int groupBegin = -1;
            int termBegin = -1;
            bool inQuotes = false;
            List<string> group = new();
            List<string> keys = new();
            List<List<string>> parameters = new();
            List<string> groupParams = null;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == '-' && i != 0 && line[i - 1] == ' ') // ' -'
                {
                    if (groupBegin > 0)
                        FinishGroup(groupBegin, end: i - 1);
                    groupBegin = i;
                    termBegin = i + 1;
                    groupParams = new();
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (termBegin > 0 && line[i - 1] != ' ')
                    {
                        if (keys.Count == group.Count)
                            FinishKey(termBegin, end: i);
                        else
                            FinishParam(termBegin, end: i);
                    }
                    termBegin = i + 1;
                }
            }

            if (termBegin > 0 && line[line.Length - 1] != ' ')
            {
                if (keys.Count == group.Count)
                    FinishKey(termBegin, end: line.Length);
                else
                    FinishParam(termBegin, end: line.Length);
            }

            if (groupBegin > 0)
                FinishGroup(groupBegin, end: line.Length);

            void FinishGroup(int begin, int end)
            {
                parameters.Add(groupParams);
                group.Add(line.Substring(begin, end - begin));
            }

            void FinishParam(int begin, int end)
            {
                groupParams.Add(line.Substring(begin, end - begin));
            }

            void FinishKey(int begin, int end)
            {
                keys.Add(line.Substring(begin, end - begin));
            }

            s_argumentsGrouped = group;
            s_argumentKeys = keys;
            s_argumentParams = parameters;
        }

        public static string CompleteCommandLine => Environment.CommandLine;

        public static bool TryGetAllParams(string key, out string[] parameters)
        {
            if (FindArgument(key, out int index))
            {
                parameters = s_argumentParams[index].ToArray();
                return true;
            }
            parameters = default;
            return false;
        }

        public static bool TryGetInt(string key, out int value)
        {
            if (FindArgument(key, out int index))
            {
                if (s_argumentParams[index].Count > 0)
                    return int.TryParse(s_argumentParams[index][0], out value);
            }
            value = default;
            return false;
        }

        public static bool TryGetFloat(string key, out float value)
        {
            if (FindArgument(key, out int index))
            {
                if (s_argumentParams[index].Count > 0)
                    return float.TryParse(s_argumentParams[index][0], NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            }
            value = default;
            return false;
        }

        public static bool TryGetString(string key, out string value)
        {
            if (FindArgument(key, out int index))
            {
                if (s_argumentParams[index].Count > 0)
                {
                    value = s_argumentParams[index][0];
                    return true;
                }
            }
            value = default;
            return false;
        }

        public static bool HasArgument(string key)
        {
            return FindArgument(key, out _);
        }

        static bool FindArgument(string key, out int index)
        {
            Init();
            index = s_argumentKeys.IndexOf(key);
            return s_argumentKeys.IsValidIndex(index);
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
