using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Services;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ChatBox.API.Domains.AIChats.Controllers;

[Controller]
[Route("api/[controller]")]
public class AIChatController(
    SessionService sessionService,
    IStickerRewardService stickerRewardService,
    IDashboardService dashboardService,
    IMessageProcessor messageProcessor) : ControllerBase
{
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] AIMessageRequestDto request)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var response = await messageProcessor.ProcessMessageAsync(request, userId);

        return Ok(response);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(string sessionName = "Đoạn chat mới")
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var session = await sessionService.CreateSessionAsync(sessionName, userId);
        return Ok(session);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([AsParameters] PaginationRequest paginationRequest)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var sessions = await sessionService.GetSessionsAsync(userId, paginationRequest);
        return Ok(sessions);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var success = await sessionService.DeleteSessionAsync(id, userId);
        return success ? Ok() : NotFound();
    }

    [HttpGet("sessions/{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [AsParameters] PaginationRequest paginationRequest)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var messages = await messageProcessor.GetMessagesAsync(id, userId, paginationRequest);
        return Ok(messages);
    }

    // [HttpPost("sessions/{id}/messages")]
    // public async Task<IActionResult> AddMessage(Guid id, [FromBody] string content)
    // {
    //     var userId = Guid.Parse(User.GetUserId());
    //     
    //     var message = await messageProcessor.AddMessageAsync(id,userId , content, senderIsEmo: false);
    //     return Ok(message);
    // }

    [HttpPut("sessions/{id}/messages/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var response = await messageProcessor.MarkMessagesAsReadAsync(id, userId);
        return Ok(response);
    }

    [HttpPost("sticker/claim")]
    public async Task<IActionResult> ClaimSticker(Guid sessionId)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var rewardMessage = await stickerRewardService.ClaimStickerAsync(userId, sessionId);

        return Ok(rewardMessage);
    }

    [HttpGet("dashboard/cohorts")]
    public async Task<IActionResult> GetChatCohorts([FromQuery] DateOnly? startDate, [FromQuery] int maxWeeks = 7,
        CancellationToken ct = default)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out _))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-56)); // mặc định 8 tuần gần đây
        var result = await dashboardService.GetChatCohortsAsync(start, maxWeeks, ct);

        return Ok(result);
    }
    
    [HttpGet("dashboard/onscreen-stats")]
    public async Task<IActionResult> GetUsersChatOnscreenStats([FromQuery] DateOnly? startDate, [FromQuery] int maxWeeks = 7,
        CancellationToken ct = default)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out _))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-56)); // mặc định 8 tuần gần đây
        var result = await dashboardService.GetUsersChatOnscreenStatsAsync(start, maxWeeks, ct);

        return Ok(result);
    }
    
    [HttpGet("dashboard/retention")]
    public async Task<IActionResult> GetRetentionReport([FromQuery] DateOnly? startDate, [FromQuery] int maxWeeks = 7,
        CancellationToken ct = default)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out _))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        // Mặc định lấy 8 tuần gần nhất (56 ngày)
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-56)); 
        
        var result = await dashboardService.GetRetentionReportAsync(start, maxWeeks, ct);

        return Ok(result);
    }
    
    
    [HttpGet("dashboard/retention-curve")]
    public async Task<IActionResult> GetRetentionCurve([FromQuery] int weeks = 12, CancellationToken ct = default)
    {
        var stats = await dashboardService.GetWeeklyRetentionCurveAsync(weeks, ct);
        return Ok(stats);
    }
}