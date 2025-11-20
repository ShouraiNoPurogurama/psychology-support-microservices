using ChatBox.API.Domains.AIChats.Services.Contracts;

namespace ChatBox.API.Domains.AIChats.Services;

public class ChatMessagePreprocessor() 
    : IMessagPreprocessor
{
    public string FormatUserMessageBlock(string userMessage)
    {
        var processedUserMessage = TrimUserMessageIfExceedsLength(userMessage);
        
        return $"[User đang nhắn]:\n {processedUserMessage}";
    }
    
    private string TrimUserMessageIfExceedsLength(string userMessage)
    {
        const int MaxUserInputLength = 2000;
        
        if (userMessage.Length <= MaxUserInputLength)
            return userMessage;

        var notice = "Ghi chú: nội dung người dùng đã được rút gọn vì quá dài.\n";
        var trimmed = userMessage[..MaxUserInputLength] + "...";
        return notice + trimmed;
    }
}