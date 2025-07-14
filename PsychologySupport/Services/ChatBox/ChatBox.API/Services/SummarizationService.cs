using System.Text;
using ChatBox.API.Data;
using ChatBox.API.Dtos.Gemini;
using ChatBox.API.Events;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatBox.API.Services;

public class SummarizationService(IOptions<GeminiConfig> config, ChatBoxDbContext dbContext, SessionService sessionService)
{
    private readonly GeminiConfig _config = config.Value;

    public async Task MaybeSummarizeSessionAsync(Guid userId, Guid sessionId)
    {
        var messagesCount = await dbContext.AIChatMessages
            .CountAsync(m => m.SessionId == sessionId);

        var session = await dbContext.AIChatSessions.FirstAsync(s => s.Id == sessionId);

        var lastIndex = session.LastSummarizedIndex ?? 0;
        
        var newMessagesCount = messagesCount - (lastIndex + 1);

        if (newMessagesCount > 10)
        {
            var newMessages = await dbContext.AIChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedDate)
                .Skip(lastIndex + 1)
                .ToListAsync();
            
            session.AddDomainEvent(new AIChatSessionSummarizedEvent(userId, sessionId, newMessages));
            
            dbContext.AIChatSessions.Update(session);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<string> CallGeminiSummarizationV1BetaAsync(List<GeminiContentDto> contents)
    {
        var apiKey = _config.ApiKey;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={apiKey}";

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = _config.SummaryInstruction } }
            },
            contents = new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text =
                            $"Tóm tắt đoạn hội thoại sau:\n{string.Join("\n", contents.Select(c => new { c.Role, c.Parts[0].Text }))}"
                    }
                }
            },
            generationConfig = new
            {
                temperature = 1.0,
                topP = 0.95,
                maxOutputTokens = 2048
            }
        };

        var json = JsonConvert.SerializeObject(payload);
        using var client = new HttpClient();
        var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var result = await response.Content.ReadAsStringAsync();

        var parsed = JObject.Parse(result);
        return parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
               ?? throw new Exception("Failed to summarize history.");
    }
    
    public async Task<bool> UpdateSessionSummarizationAsync(Guid userId, Guid sessionId, string summary, int newMessageCount)
    {
        var session = await sessionService.GetSessionAsync(userId, sessionId);
        var persona = session.PersonaSnapshot;

        if (persona is not null)
        {
            var sessionSummarizations = session.Summarization ?? "";
            var lastTwo = sessionSummarizations
                .Split("\n---\n", StringSplitOptions.RemoveEmptyEntries)
                .TakeLast(2)
                .ToList();
            

            //Tạo block mới cho bản tóm tắt hiện tại
            lastTwo.Add(summary);

            while (lastTwo.Count > 2)
                lastTwo.RemoveAt(0);

            session.Summarization = string.Join("\n---\n", lastTwo);
        }
        else
        {
            //Nếu không có persona, chỉ lưu tóm tắt như bình thường
            var sessionSummarizations = session.Summarization ?? "";
            var lastTwo = sessionSummarizations
                .Split("\n---\n", StringSplitOptions.RemoveEmptyEntries)
                .TakeLast(2)
                .ToList();

            lastTwo.Add(summary);
            while (lastTwo.Count > 2)
                lastTwo.RemoveAt(0);

            session.Summarization = string.Join("\n---\n", lastTwo);
        }

        session.LastSummarizedAt = DateTime.UtcNow;
        session.LastSummarizedIndex += newMessageCount;

        await sessionService.UpdateSessionAsync(session);
        
        return true;
    }

}