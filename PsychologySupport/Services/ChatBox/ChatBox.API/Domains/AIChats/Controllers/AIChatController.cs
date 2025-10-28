using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Services;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ChatBox.API.Domains.AIChats.Controllers;

[Controller]
[Route("api/[controller]")]
public class AIChatController(SessionService sessionService, IStickerRewardService stickerRewardService, IMessageProcessor messageProcessor) : ControllerBase
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

        await messageProcessor.MarkMessagesAsReadAsync(id, userId);
        return Ok();
    }
    
    [HttpPost("sticker/claim")] 
    public async Task<IActionResult> ClaimSticker()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedException("Token không hợp lệ: Không tìm thấy userId.", "CLAIMS_MISSING");
        }

        var rewardMessage = await stickerRewardService.ClaimStickerAsync(userId);
        
        return Ok(rewardMessage);
    }
}