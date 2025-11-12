using System.Net.Http.Headers;
using System.Text;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiProvider(
    IOptions<GeminiConfig> config,
    ILogger<GeminiProvider> logger,
    IConfiguration cfg)
    : IAIProvider
{
    // Trained model (Vertex AI)
    public async Task<string> GenerateFineTunedChatResponseAsync(AIRequestPayload payload)
    {
        var geminiPayload = ToGeminiRequest(payload, systemInstruction: config.Value.SystemInstruction);

        return await CallFineTunedGeminiAPIAsync(geminiPayload);
    }

    // Foundational model (API key)
    public async Task<string> GenerateChatResponseAsync(AIRequestPayload payload,
        string systemInstruction,
        CancellationToken ct = default)
    {
        var request = ToGeminiRequest(payload, systemInstruction: systemInstruction);

        return await CallGeminiFlashLiteInternalAsync(request, "Gemini Flash Lite", ct);
    }

    public async Task<string> CallGeminiStructuredOutputAPIAsync(GeminiStructuredOutputRequestDto payload, CancellationToken ct = default)
    {
        return await CallGeminiFlashLiteInternalAsync(payload, "Gemini Structured", ct);
    }

    public async Task<string> CallGeminiOutputAPIAsync(GeminiRequestDto payload, CancellationToken ct = default)
    {
        return await CallGeminiFlashLiteInternalAsync(payload, "Gemini Text", ct);
    }

    // === Mapping duy nhất: AIRequestPayload -> GeminiRequestDto
    private static GeminiRequestDto ToGeminiRequest(AIRequestPayload payload, string systemInstruction)
    {
        var contents = new List<GeminiContentDto>();

        if (!string.IsNullOrWhiteSpace(payload.Summarization))
        {
            contents.Add(new GeminiContentDto("user",
                [new GeminiContentPartDto(payload.Summarization)]));
        }

        if (payload.HistoryMessages is { Count: > 0 })
        {
            contents.AddRange(payload.HistoryMessages.Select(h =>
                new GeminiContentDto(h.IsFromAI ? "model" : "user",
                    [new GeminiContentPartDto(h.Content)])));
        }

        contents.Add(new GeminiContentDto("user",
            [new GeminiContentPartDto(payload.Context)]));

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(systemInstruction)),
            GenerationConfig: new GeminiGenerationConfigDto(Temperature: 1, TopP: 0.95, MaxOutputTokens: 8192),
            SafetySettings:
            [
                new("HARM_CATEGORY_HATE_SPEECH"),
                new("HARM_CATEGORY_DANGEROUS_CONTENT"),
                new("HARM_CATEGORY_SEXUALLY_EXPLICIT"),
                new("HARM_CATEGORY_HARASSMENT")
            ]
        );
    }

    private async Task<string> CallGeminiFlashLiteInternalAsync<TRequest>(
        TRequest payload,
        string logContext,
        CancellationToken ct)
    {
        using var http = new HttpClient();

        var apiKey = cfg["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        var serializedPayload = JsonConvert.SerializeObject(payload);
        logger.LogInformation("{Context} Payload: {Payload}", logContext, serializedPayload);

        var content = new StringContent(serializedPayload, Encoding.UTF8, "application/json");
        var resp = await http.PostAsync(url, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("{Context} call failed: {StatusCode} - {Body}", logContext, resp.StatusCode, body);
            return string.Empty;
        }

        // Log thêm response cho đồng bộ
        logger.LogInformation("{Context} Response: {Body}", logContext, body);

        var jo = JObject.Parse(body);
        var text = jo["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "";
        return text.Trim();
    }

    // ===== Gọi model qua Vertex AI (service account token) =====
    private async Task<string> CallFineTunedGeminiAPIAsync(GeminiRequestDto payload)
    {
        var token = await GetAccessTokenAsync();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var configValue = config.Value;
        var url =
            $"https://{configValue.Location}-aiplatform.googleapis.com/v1/projects/{configValue.ProjectId}/locations/{configValue.Location}/endpoints/{configValue.EndpointId}:streamGenerateContent";

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload, settings), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        var array = JArray.Parse(result);
        var texts = array
            .Select(token => token["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString())
            .Where(text => !string.IsNullOrEmpty(text))
            .ToList();

        return string.Join("", texts);
    }


    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}