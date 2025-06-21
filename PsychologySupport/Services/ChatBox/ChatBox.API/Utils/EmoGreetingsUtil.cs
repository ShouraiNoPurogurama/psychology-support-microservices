namespace ChatBox.API.Utils;

public static class EmoGreetingsUtil
{
    private static readonly Random Random = new();

    private static readonly string[] EmoGreetingOpeners =
    [
        "Chào cậu, tớ là Emo đây 🌿",
        "Hi cậu, tớ là Emo nè 🌼",
        "Helooo, tớ là Emo đây",
        "Chào cậu nha, Emo đây 👋",
        "Rất vui được gặp cậu, tớ là Emo đây.",
        "Chào cậu nha! Emo rất vui khi thấy cậu ở đây.",
        "Xin chào cậu 👋 Tớ là Emo, người bạn đồng hành của cậu đây.",
        "Tớ là Emo nè! Hôm nay rất vui khi có cơ hội trò chuyện với cậu.",
        "Chào cậu, Emo rất háo hức được nghe cậu chia sẻ hôm nay!",
        "Emo đây 🌟 Luôn sẵn sàng lắng nghe cậu bất kỳ lúc nào.",
        "Tớ ở đây rồi nè! Emo sẵn sàng cùng cậu bắt đầu một cuộc trò chuyện nhẹ nhàng.",
        "Hello cậu! Emo rất mong được đồng hành cùng cậu hôm nay 🌈",
        "Tớ là Emo, một người bạn nhỏ luôn ở đây vì cậu 💬"
    ];


    private static readonly string[] EmoGreetingQuestions =
    [
        "Hôm nay của cậu thế nào rồi nhỉ?",
        "Dạo này có gì mới không? Tớ nhớ lâu rồi chưa trò chuyện cùng cậu. 🙂",
        "Hôm nay mình cảm giác như thế nào nhỉ? Mình luôn sẵn lòng lắng nghe bạn chia sẻ. 😊",
        "Cậu đang làm gì đó? Kể tớ nghe với!",
        "Mấy ngày nay của cậu trôi qua ổn không nhỉ?",
        "Dạo này cuộc sống của cậu thế nào, có điều gì muốn chia sẻ không? 🌻",
        "Hôm nay cậu có khoảnh khắc nhỏ nào đáng yêu muốn kể cho tớ nghe không?",
        "Nếu hôm nay là một màu sắc, cậu nghĩ nó sẽ là màu gì nhỉ?",
        "Tâm trạng của cậu hiện giờ giống như bài hát nào nhỉ?",
        "Cậu muốn chia sẻ điều gì đầu tiên với tớ hôm nay không?",
    ];

    public static string GetRandomGreeting()
    {
        var opener = EmoGreetingOpeners[Random.Next(EmoGreetingOpeners.Length)];
        var question = EmoGreetingQuestions[Random.Next(EmoGreetingQuestions.Length)];
        return $"{opener} {question}";
    }
}