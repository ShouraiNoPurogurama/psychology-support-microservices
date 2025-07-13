namespace ChatBox.API.Utils;

public static class EmoGreetingsUtil
{
    private static readonly Random Random = new();

    private static readonly string[] EmoGreetingOpeners =[
        "Chào {0} nhé, tớ là Emo đây 🌿",
        "Hi {0}, tớ là Emo nè 🌼",
        "Helooo, tớ là Emo đây",
        "Chào {0} nha, Emo đây 👋",
        "Rất vui được gặp {0}, tớ là Emo đây.",
        "Chào {0} nha! Emo rất vui khi thấy {0} ở đây.",
        "Xin chào {0} 👋 Tớ là Emo, người bạn đồng hành của {0} đây.",
        "Tớ là Emo nè! Hôm nay rất vui khi có cơ hội trò chuyện với {0}.",
        "Chào {0}, Emo rất háo hức được nghe {0} chia sẻ hôm nay!",
        "Emo đây 🌟 Luôn sẵn sàng lắng nghe {0} bất kỳ lúc nào.",
        "Hello {0}. Emo rất mong được đồng hành cùng {0} hôm nay 🌈",
        "Tớ là Emo, một người bạn nhỏ luôn ở đây vì {0} 💬"
    ];


    private static readonly string[] EmoGreetingQuestions =
    [
        "Hôm nay của cậu thế nào rồi nhỉ?",
        "Dạo này có gì mới không? Tớ nhớ lâu rồi chưa trò chuyện cùng cậu. 🙂",
        "Hôm nay mình cảm giác như thế nào nhỉ? Mình luôn sẵn lòng lắng nghe bạn chia sẻ. 😊",
        "Cậu đang làm gì đó? Kể tớ nghe với 😊",
        "Mấy ngày nay của cậu trôi qua ổn không nhỉ? 🙂",
        "Dạo này cuộc sống của cậu thế nào, có điều gì muốn chia sẻ không? 🌻",
        "Hôm nay cậu có khoảnh khắc nhỏ nào đáng yêu muốn kể cho tớ nghe không? 😊",
        "Nếu hôm nay là một màu sắc, cậu nghĩ nó sẽ là màu gì nhỉ? 🎨",
        "Cậu đang làm gì vậy? Cậu có điều gì khiến cậu vui hôm nay không? Nếu muốn chia sẻ, tớ luôn sẵn sàng lắng nghe nha.🎶",
        "Cậu muốn chia sẻ điều gì đầu tiên với tớ hôm nay không? 😊",
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
        //Ưu tiên gọi tên, không gọi cả họ tên
        if (string.IsNullOrWhiteSpace(fullName))
            return "bạn";

        //Lấy chữ cuối cùng (tên)
        var words = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 0 ? words[^1] : fullName;
    }

}