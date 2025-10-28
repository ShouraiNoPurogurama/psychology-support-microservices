using ChatBox.API.Models;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IContextBuilder
{
    Task<string> BuildContextAsync(Guid sessionId, string userMessage);
    Task<List<AIMessage>> GetLastEmoMessageBlock(Guid sessionId);

}