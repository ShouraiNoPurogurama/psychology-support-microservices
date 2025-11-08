using System.Text.RegularExpressions;

namespace ChatBox.API.Domains.AIChats.Utils;

public static class EmoGreetingsUtil
{
    private static readonly Random Random = new();

    // CẬP NHẬT: Mảng chào này tập trung vào sự kết nối và
    // khơi gợi (theo góp ý mới nhất)
    private static readonly string[] EmoOnboardingGreetings = [
        "Hi {0}! Tớ là Emo. Tớ rất vui vì cuối cùng cậu cũng tìm đến. 🤗",
        "{0} ơi! Emo đây. Tớ đã chờ để được kết nối với cậu đó. ✨",
        "Chào {0} nè! Emo đây. Tớ đang sắp xếp lại mấy suy nghĩ của mình... may mà có cậu tới chơi. 😉",
        "A, {0} ơi! Tớ là Emo. Cậu xuất hiện làm ngày của tớ thú vị hơn hẳn. 🌼",
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
            return "bạn";

        var cleaned = Regex.Replace(fullName, @"[\(\[].*?[\)\]]", "");
        var normalized = Regex.Replace(cleaned.Trim(), @"\s+", " ");
        var words = normalized.Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries);

        return words.Length switch
        {
            0 => "bạn",
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