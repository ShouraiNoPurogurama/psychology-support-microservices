using System.Net.Http.Headers;
using System.Text;
using ChatBox.API.Domains.AIChats.Abstractions;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiInstructionGenerator : IInstructionGenerator
{
    private IConfiguration config;
    private readonly GeminiConfig _config;
    private readonly ILogger<GeminiInstructionGenerator> _logger;
    
    public GeminiInstructionGenerator(IOptions<GeminiConfig> config, ILogger<GeminiInstructionGenerator> logger, IConfiguration configuration)
    {
        _config = config.Value;
        _logger = logger;
        this.config = configuration;
    }

    public async Task<string> GenerateInstructionAsync(string userMessage, string? history = null, string? persona = null)
    {
        try
        {
            var payload = BuildPayload(userMessage, history, persona);
            var instruction = await CallGeminiAPIAsync(payload);
            
            _logger.LogInformation("Generated instruction: {Instruction}", instruction);
            
            return string.IsNullOrWhiteSpace(instruction) ? string.Empty : instruction.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate instruction for Emo.");
            return string.Empty; // Fail silently to not break the main chat flow
        }
    }

    private GeminiRequestDto BuildPayload(string userMessage, string? history, string? persona)
    {
        string systemInstruction = config["GeminiConfig:InstructorInstruction"];
        
        var contextParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(persona)) contextParts.Add($"USER PERSONA:\n{persona}");
        if (!string.IsNullOrWhiteSpace(history)) contextParts.Add($"CONVERSATION SUMMARY:\n{history}");
        contextParts.Add($"CURRENT USER MESSAGE:\n{userMessage}");

        var fullContext = string.Join("\n\n", contextParts);

        return new GeminiRequestDto(
            Contents: [new GeminiContentDto("user", [new GeminiContentPartDto(fullContext)])],
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(systemInstruction)),
            GenerationConfig: new GeminiGenerationConfigDto(
                Temperature: 0.5, // Lower temperature for more deterministic instructions
                MaxOutputTokens: 200 // Instructions should be short
            ),
            SafetySettings:
            [
                new("HARM_CATEGORY_HATE_SPEECH"),
                new("HARM_CATEGORY_DANGEROUS_CONTENT"),
                new("HARM_CATEGORY_SEXUALLY_EXPLICIT"),
                new("HARM_CATEGORY_HARASSMENT")
            ]
        );
    }
    
    // NOTE: This is duplicated from GeminiProvider. In a real scenario, you might refactor this into a shared HttpClient service.
    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var token = await GetAccessTokenAsync();
        using var client = new HttpClient();
        // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var apiKey = config["GeminiConfig:ApiKey"];
        var url =  $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite-preview-06-17:generateContent?key={apiKey}";

        var settings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } };
        var content = new StringContent(JsonConvert.SerializeObject(payload, settings), Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API call for instruction failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, result);
            return string.Empty;
        }

        var jObject = JObject.Parse(result);
        var text = jObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

        return text ?? string.Empty;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}