using System.Text;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiInstructionGenerator : IInstructionGenerator
{
    private IConfiguration configuration;
    private readonly IAIProvider _aiProvider;
    private readonly GeminiConfig _config;
    private readonly ILogger<GeminiInstructionGenerator> _logger;
    
    public GeminiInstructionGenerator(IOptions<GeminiConfig> config, ILogger<GeminiInstructionGenerator> logger, IConfiguration configuration, IAIProvider aiProvider)
    {
        _config = config.Value;
        _logger = logger;
        this.configuration = configuration;
        _aiProvider = aiProvider;
    }

    public async Task<string> GenerateInstructionAsync(string userMessage, string? history = null, string? persona = null)
    {
        try
        {
            var payload = BuildPayload(userMessage, history, persona);
            var instruction = await _aiProvider.CallGeminiOutputAPIAsync(payload);
            
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
        string systemInstruction = configuration["GeminiConfig:InstructorInstruction"];
        
        var contextParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(persona)) contextParts.Add($"USER PERSONA:\n{persona}");
        if (!string.IsNullOrWhiteSpace(history)) contextParts.Add($"CONVERSATION SUMMARY:\n{history}");
        contextParts.Add($"USER MESSAGE:\n{userMessage}");

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
    

    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}