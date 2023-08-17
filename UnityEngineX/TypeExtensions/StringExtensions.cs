namespace UnityEngineX
{
    public static class StringExtensions
    {
        public static int ParseInt(this string txt, int index) => txt.ParseInt(ref index);

        public static int ParseInt(this string txt, ref int index)
        {
            int begin = index;
            while (index < txt.Length && char.IsDigit(txt[index]))
            {
                index++;
            }

            return int.Parse(txt.Substring(begin, index - begin));
        }

        public static bool TryParseInt(this string txt, int index, out int result) => txt.TryParseInt(ref index, out result);

        public static bool TryParseInt(this string txt, ref int index, out int result)
        {
            int begin = index;
            while (index < txt.Length && char.IsDigit(txt[index]))
            {
                index++;
            }

            return int.TryParse(txt.Substring(begin, index - begin), out result);
        }

        public static string TrimEnd(this string txt, int index)
        {
            if (index >= txt.Length)
                return txt;
            return txt.Remove(index, txt.Length - index);
        }

        public static string ReplacePortion(this string text, int index, int count, string replacement)
        {
            return text.Remove(index, count).Insert(index, replacement);
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string text, string search, string replace)
        {
            int pos = text.LastIndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Remove the first met instance of the given substring
        /// </summary>
        public static string RemoveFirst(this string text, string subString)
        {
            int index = text.IndexOf(subString);

            if (index != -1)
            {
                return text.Remove(index, subString.Length);
            }

            return text;
        }

        /// <summary>
        /// Remove the last met instance of the given substring
        /// </summary>
        public static string RemoveLast(this string text, string subString)
        {
            int index = text.LastIndexOf(subString);

            if (index != -1)
            {
                return text.Remove(index, subString.Length);
            }

            return text;
        }

        public static string ToHashSHA2(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return System.BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
