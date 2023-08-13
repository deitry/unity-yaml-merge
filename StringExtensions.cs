namespace Tests;

public static class StringExtensions
{
    public static string[] CharsToStringArray(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return Array.Empty<string>();

        return input.ToCharArray().Select(c => c.ToString()).ToArray();
    }
}
