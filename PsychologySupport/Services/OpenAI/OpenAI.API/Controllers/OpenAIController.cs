using Microsoft.AspNetCore.Mvc;
using OpenAI.API.Dtos;
using OpenAI.API.Services;
using System.Text.Json;

namespace OpenAI.API.Controllers
{
    [Route("api/openai")]
    [ApiController]
    public class OpenAIController(IConfiguration configuration, OpenAIService openAiService) : ControllerBase
    {
        [HttpPost("generate-plan")]
        public async Task<IActionResult> GeneratePlanAsync([FromBody] ScheduleRequest request)
        {
            var jsonSchedule = JsonSerializer.Serialize(request);
            var result = await openAiService.GeneratePlanAsync(jsonSchedule);
            
            return Ok(new { plan = result });
        }
    }
}