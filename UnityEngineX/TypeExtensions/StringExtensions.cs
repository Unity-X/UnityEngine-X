
public static class StringExtensions
{
    public static string TrimEnd(this string txt, int index)
    {
        if (index >= txt.Length)
            return txt;
        return txt.Remove(index, txt.Length - index);
    }
    
    /// <summary>
    /// Remove the first met instance of the given substring
    /// </summary>
    public static string RemoveFirst(this string txt, string subString)
    {
        int index = txt.IndexOf(subString);

        if(index != -1)
        {
            return txt.Remove(index, subString.Length);
        }

        return txt;
    }
}
