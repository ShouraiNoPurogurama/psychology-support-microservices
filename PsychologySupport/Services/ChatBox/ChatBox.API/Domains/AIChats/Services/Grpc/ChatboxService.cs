using ChatBox.API.Data;
using Chatbox.API.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Pii.API.Protos;

namespace ChatBox.API.Domains.AIChats.Services.Grpc;

public class ChatboxService(PiiService.PiiServiceClient piiClient, ILogger<ChatboxService> logger, ChatBoxDbContext dbContext)
    : Chatbox.API.Protos.ChatboxService.ChatboxServiceBase
{
    public override async Task<GetDailySummaryResponse> GetDailySummary(GetDailySummaryRequest request, ServerCallContext context)
    {
        var response = await piiClient.ResolveUserIdByAliasIdAsync(new ResolveUserIdByAliasIdRequest
        {
            AliasId = request.AliasId
            
        });

        var userId = Guid.Parse(response.UserId);
        var sessionId = Guid.Parse(request.ChatSessionId);

        // 1) Chọn timezone logic (ngày của user)
        var tz = TryGetTimeZone(
            windowsId: "SE Asia Standard Time",     // Windows
            ianaId: "Asia/Ho_Chi_Minh"              // Linux/macOS
        );

        // 2) Lấy "hôm nay" theo giờ địa phương
        var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
        var startLocal = new DateTimeOffset(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, nowLocal.Offset);
        var endLocal   = startLocal.AddDays(1);

        // 3) Quy về UTC để so với CreatedDate (timestamptz)
        var startUtc = startLocal.ToUniversalTime();
        var endUtc   = endLocal.ToUniversalTime();

        var chatSession = await dbContext.AIChatSessions
            .Where(s => s.IsActive == true
                        && s.UserId == userId
                        && s.Id == sessionId
                        //&& s.CreatedDate >= startUtc
                        //&& s.CreatedDate <  endUtc
            )
            .OrderByDescending(s => s.CreatedDate)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (chatSession is null)
        {
            logger.LogWarning(
                "No active chat session for AliasId:{AliasId} UserId:{UserId} in local day {Day} ({StartUtc}..{EndUtc} UTC)",
                request.AliasId, userId, startLocal.Date, startUtc, endUtc
            );
        }

        return new GetDailySummaryResponse
        {
            SummarizationJson   = chatSession?.Summarization ?? string.Empty,
            PersonaSnapshotJson = chatSession?.PersonaSnapshotJson ?? string.Empty,
            UserId =chatSession?.UserId.ToString() ?? string.Empty,
        };
    }

    private static TimeZoneInfo TryGetTimeZone(string windowsId, string ianaId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(ianaId); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById(windowsId); }
    }

}