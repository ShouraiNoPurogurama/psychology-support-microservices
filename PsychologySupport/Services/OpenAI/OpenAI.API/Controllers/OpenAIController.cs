using Microsoft.AspNetCore.Mvc;
using OpenAI.API.Dtos;
using OpenAI.API.Services;
using System.Text.Json;

namespace OpenAI.API.Controllers
{
    [Route("api/openai")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly OpenAIService _openAIService;

        public OpenAIController(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        [HttpPost("generate-plan")]
        public async Task<IActionResult> GeneratePlanAsync([FromBody] ScheduleRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request payload.");
            }

            var jsonSchedule = JsonSerializer.Serialize(request);
            var result = await _openAIService.GeneratePlanAsync(jsonSchedule);

            return Ok(new { plan = result });
        }
    }
}