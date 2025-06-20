using ChatBox.API.Dtos;
using ChatBox.API.Extensions;
using ChatBox.API.Models;
using ChatBox.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatBox.API.Controllers;

[Controller]
[Route("api/[controller]")]
public class AIChatController(GeminiService geminiService) : ControllerBase
{
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] AIMessageRequestDto request)
    {
        var userId = User.GetUserId();
        
        var response = await geminiService.GenerateAsync(request, Guid.Parse(userId));

        return Ok(response);
    }
}