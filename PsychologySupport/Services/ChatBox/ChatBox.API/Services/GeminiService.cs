using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using ChatBox.API.Data;
using ChatBox.API.Dtos;
using ChatBox.API.Dtos.Gemini;
using ChatBox.API.Extensions;
using ChatBox.API.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ChatBox.API.Services;

public class GeminiService(IOptions<GeminiConfig> config, ChatBoxDbContext dbContext)
{
    private readonly GeminiConfig _config = config.Value;

    public async Task<AIMessageResponseDto> GenerateAsync(AIMessageRequestDto request, Guid userId)
    {
        var history = request.History;
        var userMessage = request.UserMessage;
        var sessionId = request.SessionId;

        var session = await dbContext.AIChatSessions.FindAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("You are not the owner of this session.");

        var token = await GetAccessTokenAsync();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url =
            $"https://{_config.Location}-aiplatform.googleapis.com/v1/projects/{_config.ProjectId}/locations/{_config.Location}/endpoints/{_config.EndpointId}:streamGenerateContent";

        var contents = history.Select(m =>
            {
                var role = m.SenderIsEmo ? "model" : "user";
                var parts = new List<GeminiContentPartDto>
                {
                    new(m.Content)
                };

                return new GeminiContentDto(role, parts);
            })
            .ToList();

        var newParts = new List<GeminiContentPartDto> { new(userMessage) };

        contents.Add(new GeminiContentDto("user", newParts));

        var payload = GenerateGeminiPayload(contents);

        var AIResponse = await PostGeminiRequestAsync(payload, client, url);

        return new AIMessageResponseDto(
            SessionId: request.SessionId,
            SenderIsEmo: true,
            Content: AIResponse,
            CreatedDate: DateTime.UtcNow
        );
    }

    private static async Task<string> PostGeminiRequestAsync(GeminiRequestDto payload, HttpClient client, string url)
    {
        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        dynamic json = JsonConvert.DeserializeObject(result);
        var parts = json?.candidates?[0]?.content?.parts;
        string text = parts?[0]?.text?.ToString() ?? string.Empty;
        return text;
    }

    private GeminiRequestDto GenerateGeminiPayload(List<GeminiContentDto> contents)
    {
        var payload = new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(
                Parts: [new GeminiContentPartDto(_config.SystemInstruction)]
            ),
            GenerationConfig: new GeminiGenerationConfigDto(
                Temperature: 1.0,
                TopP: 0.95,
                MaxOutputTokens: 8192
            ),
            SafetySettings:
            [
                new("HARM_CATEGORY_HATE_SPEECH"),
                new("HARM_CATEGORY_DANGEROUS_CONTENT"),
                new("HARM_CATEGORY_SEXUALLY_EXPLICIT"),
                new("HARM_CATEGORY_HARASSMENT")
            ]
        );
        return payload;
    }


    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}