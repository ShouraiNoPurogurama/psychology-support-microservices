namespace BuildingBlocks.Utils;

public static class StringUtils
{
    public static string GetSnippet(string input, int length = 25)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        if (input.Length <= length)
        {
            return input;
        }

        return input.Substring(0, length) + "...";
    }
}