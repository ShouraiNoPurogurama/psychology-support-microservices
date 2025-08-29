using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Abstractions;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

public class ChatContextBuilder(ChatBoxDbContext dbContext, ILogger<ChatContextBuilder> logger) 
    : IContextBuilder
{
    public async Task<string> BuildContextAsync(Guid sessionId, string userMessage)
    {
        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .FirstAsync(s => s.Id == sessionId);

        var persona = session.PersonaSnapshot.ToPromptText();
        var contextBlock = await BuildContextBlock(sessionId);
        var processedUserMessage = TrimUserMessageIfExceedsLength(userMessage);

        return $"{persona}{contextBlock}[User]\n{processedUserMessage}\n\n[Emo]:\n";
    }

    private async Task<string> BuildContextBlock(Guid sessionId)
    {
        var lastMessageBlock = await GetLastEmoMessageBlock(sessionId);
        
        if (lastMessageBlock.Count == 0)
            return "";

        var contextContent = string.Join("\n", lastMessageBlock
            .OrderBy(m => m.CreatedDate)
            .Select(m => m.Content));

        return $"[Previous Context Messages By Emo]\n{contextContent}\n\n";
    }

    private string TrimUserMessageIfExceedsLength(string userMessage)
    {
        const int MaxUserInputLength = 1000;
        
        if (userMessage.Length <= MaxUserInputLength)
            return userMessage;

        var notice = "Ghi chú: nội dung người dùng đã được rút gọn vì quá dài.\n";
        var trimmed = userMessage[..MaxUserInputLength] + "...";
        return notice + trimmed;
    }

    public async Task<List<AIMessage>> GetLastEmoMessageBlock(Guid sessionId)
    {
        var lastBlockIndex = await GetLastMessageBlockIndex(sessionId);

        while (lastBlockIndex >= 0)
        {
            var messages = await dbContext.AIChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId && m.SenderIsEmo && m.BlockNumber == lastBlockIndex)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();
            
            if (messages.Count != 0)
                return messages;

            lastBlockIndex--;
        }
        return [];
    }

    private async Task<int> GetLastMessageBlockIndex(Guid sessionId)
    {
        return await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.BlockNumber)
            .Select(m => m.BlockNumber)
            .FirstOrDefaultAsync();
    }
}