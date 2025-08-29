using System.Text.RegularExpressions;

namespace ChatBox.API.Domains.AIChats.Utils;

public static class EmoGreetingsUtil
{
    private static readonly Random Random = new();

    private static readonly string[] EmoGreetingOpeners = [
        "Chào {0} nèee ~",
        "Tớ là Emo đâyy, {0} ơi.",
        "Hi {0}, Emo tới rồi nè.",
        "He luu, {0} khỏe không? Emo đây!",
        "{0} ơi, Emo đây nè.",
        "Emo chào {0} nhaaa.",
        "Tớ đây, Emo đang nghe {0} nè.",
        "Alo alo, Emo tới rồi nè {0}!",
        "Này {0} ơi, Emo xuất hiện rồi nè."
    ];

    private static readonly string[] EmoGreetingQuestions = [
        "Ngày hôm nay của cậu thế nào á?",
        "Có tâm sự gì muốn kể tớ nghe không? 🥰",
        "Dạo này cậu ổn không nhỉ? 😄",
        "Trò chuyện cùng tớ tí không nào? 😊",
        "Có chuyện gì vui kể tớ với nha 😄",
        "Nếu có điều gì muốn chia sẻ, tớ sẵn sàng nghe luôn 📞",
        "Dạo này có gì mới không nè? 😊",
        "Cậu đang cảm thấy sao rồi? 😊",
        "Ngày hôm nay của cậu có gì đặc biệt không? 😄",
        "Tâm trạng hôm nay ra sao rồi cậu? 😄"
    ];

    
    // Nếu opener không chứa {0} thì sẽ không chèn tên, giữ nguyên.
    public static string GetRandomGreeting(string? fullName)
    {
        var opener = EmoGreetingOpeners[Random.Next(EmoGreetingOpeners.Length)];
        var question = EmoGreetingQuestions[Random.Next(EmoGreetingQuestions.Length)];
        var displayName = GetDisplayName(fullName);

        //Nếu opener có {0}, format vào; không thì giữ nguyên opener
        if (opener.Contains("{0}"))
            opener = string.Format(opener, displayName);

        return $"{opener} {question}";
    }
    
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