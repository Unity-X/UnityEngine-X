
public static class StringExtensions
{
    public static string TrimEnd(this string txt, int index)
    {
        if (index >= txt.Length)
            return txt;
        return txt.Remove(index, txt.Length - index);
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
    public static string ReplaceFirst(this string text, string subString)
    {
        int index = text.IndexOf(subString);

        if (index != -1)
        {
            return text.Remove(index, subString.Length);
        }

        return text;
    }

    /// <summary>
    /// Remove the first met instance of the given substring
    /// </summary>
    public static string RemoveFirst(this string text, string subString)
    {
        int index = text.IndexOf(subString);

        if(index != -1)
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
}
