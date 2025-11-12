using System.Text.RegularExpressions;

namespace ChatBox.API.Domains.AIChats.Utils;

public static class EmoGreetingsUtil
{
    private static readonly Random Random = new();
    private static readonly string[] EmoOnboardingGreetings = [
        "Hi {0}! Cảm ơn cậu đã tìm đến đây với tớ. Tớ rất vui được gặp cậu. 😊",

        "Chào {0}. Tớ là Emo. Cảm ơn cậu đã bắt đầu cuộc trò chuyện này. Tớ rất vui vì cuối cùng cũng được gặp cậu."
    ];


    // Hàm này CHỈ trả về 1 lời chào duy nhất, không kèm câu hỏi hay Lore.
    public static string GetOnboardingMessage(string? fullName)
    {
        var greeting = EmoOnboardingGreetings[Random.Next(EmoOnboardingGreetings.Length)];
        var displayName = GetDisplayName(fullName);

        if (greeting.Contains("{0}"))
            greeting = string.Format(greeting, displayName);

        return greeting;
    }
    
    // (Các hàm GetDisplayName, Capitalize giữ nguyên)
    private static string GetDisplayName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "cậu";

        var cleaned = Regex.Replace(fullName, @"[\(\[].*?[\)\]]", "");
        var normalized = Regex.Replace(cleaned.Trim(), @"\s+", " ");
        var words = normalized.Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries);

        return words.Length switch
        {
            0 => "cậu",
            1 => Capitalize(words[0]),
            _ => Capitalize(words[^1])
        };
    }

    private static string Capitalize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }
}