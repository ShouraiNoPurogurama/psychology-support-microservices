// Domains/AIChats/Services/AIRequestFactory.cs

using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Sessions;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services
{
    public class AIRequestFactory : IAIRequestFactory
    {
        private readonly ChatBoxDbContext _db;
        private readonly ILogger<AIRequestFactory> _logger;

        public AIRequestFactory(ChatBoxDbContext db, ILogger<AIRequestFactory> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AIRequestPayload> CreateAsync(
            List<HistoryMessage> historyMessages,
            AIChatSession session,
            string augmentedContext)
        {
            var summarization = await GetSessionSummarizationAsync(session.Id);

            _logger.LogInformation("Summarization: {Summarization}", summarization);
            _logger.LogInformation("HistoryMessages count: {Count}", historyMessages?.Count ?? 0);

            return new AIRequestPayload(
                Context: augmentedContext,
                Summarization: summarization,
                HistoryMessages: historyMessages
            );
        }

        private async Task<string?> GetSessionSummarizationAsync(Guid sessionId)
        {
            var raw = await _db.AIChatSessions
                .AsNoTracking()
                .Where(s => s.Id == sessionId)
                .Select(s => s.Summarization)
                .FirstOrDefaultAsync();

            return BuildChatSummaryBlock(raw);
        }

        private static string BuildChatSummaryBlock(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            var timeTag = $"[Thời gian hiện tại: {now:yyyy-MM-dd HH:mm}]";

            var parts = raw.Split(["---"], StringSplitOptions.RemoveEmptyEntries);

            var parsed = parts.Select(p =>
            {
                try
                {
                    var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<SummaryDto>(p.Trim().Replace("\n", ""));
                    return dto == null ? null : new { dto.Current, dto.Persist, dto.CreatedAt };
                }
                catch { return null; }
            })
            .Where(x => x != null)
            .ToList();

            var recent = parsed.TakeLast(3).ToList();

            var lines = new List<string>();
            foreach (var s in recent)
            {
                if (!string.IsNullOrWhiteSpace(s!.Current))
                    lines.Add($"- Ngữ cảnh tạm thời (điều đang diễn ra lúc đó): {s.Current}");
                if (!string.IsNullOrWhiteSpace(s.Persist))
                    lines.Add($"- Ngữ cảnh dài hạn (thông tin mang tính bền vững): {s.Persist}");
                if (s.CreatedAt != null)
                    lines.Add($"- Thời điểm được ghi nhận: {s.CreatedAt:yyyy-MM-dd}");
            }

            return $"{timeTag}\nTóm tắt trước đó:\n" + string.Join("\n", lines);
        }
    }
}
