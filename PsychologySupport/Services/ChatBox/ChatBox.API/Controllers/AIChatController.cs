using BuildingBlocks.Pagination;
using ChatBox.API.Dtos;
using ChatBox.API.Extensions;
using ChatBox.API.Models;
using ChatBox.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatBox.API.Controllers;

[Controller]
[Route("api/[controller]")]
public class AIChatController(GeminiService geminiService, SessionService sessionService) : ControllerBase
{
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] AIMessageRequestDto request)
    {
        var userId = Guid.Parse(User.GetUserId());
        
        var response = await geminiService.SendMessageAsync(request, userId);

        return Ok(response);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(string sessionName = "Đoạn chat mới")
    {
        var userId = Guid.Parse(User.GetUserId());
        var profileId = Guid.Parse(User.GetProfileId());

        var session = await sessionService.CreateSessionAsync(sessionName, userId, profileId);
        return Ok(session);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([AsParameters] PaginationRequest paginationRequest)
    {
        var userId = Guid.Parse(User.GetUserId());

        var sessions = await sessionService.GetSessionsAsync(userId, paginationRequest);
        return Ok(sessions);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var userId = Guid.Parse(User.GetUserId());

        var success = await sessionService.DeleteSessionAsync(id, userId);
        return success ? Ok() : NotFound();
    }

    [HttpGet("sessions/{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [AsParameters] PaginationRequest paginationRequest)
    {
        var userId = Guid.Parse(User.GetUserId());

        var messages = await geminiService.GetMessagesAsync(id, userId, paginationRequest);
        return Ok(messages);
    }

    [HttpPost("sessions/{id}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] string content)
    {
        var userId = Guid.Parse(User.GetUserId());
        
        var message = await geminiService.AddMessageAsync(id,userId , content, senderIsEmo: false);
        return Ok(message);
    }

    [HttpPut("sessions/{id}/messages/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.GetUserId());
        
        await geminiService.MarkMessagesAsReadAsync(id, userId);
        return Ok();
    }
}