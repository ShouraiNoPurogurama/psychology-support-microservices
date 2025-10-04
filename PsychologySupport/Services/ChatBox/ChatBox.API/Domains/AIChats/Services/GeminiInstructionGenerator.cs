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
    private readonly GeminiConfig _config;
    private readonly ILogger<GeminiInstructionGenerator> _logger;
    private const string SystemInstruction =
        "Bạn là một chuyên gia huấn luyện hội thoại cho trợ lý AI tên Emo, một người bạn đồng hành chữa lành. " +
        "Dựa trên tin nhắn và ngữ cảnh của người dùng, công việc DUY NHẤT của bạn là cung cấp một hướng dẫn ngắn gọn, chỉ một dòng, cho Emo về cách phản hồi. " +
        "Hướng dẫn phải được đặt trong dấu ngoặc vuông, ví dụ: [Gợi ý trả lời: ...]. " +
        "Tập trung vào sắc thái cảm xúc và bước tiếp theo hợp lý trong một cuộc trò chuyện hỗ trợ. " +
        "Ví dụ:\n" +
        "- Người dùng đang than phiền về một ngày tồi tệ -> [Gợi ý trả lời: Xác nhận cảm xúc của họ, thể hiện sự đồng cảm và hỏi xem họ có muốn nói thêm về điều đó không.]\n" +
        "- Người dùng đang hỏi lời khuyên -> [Gợi ý trả lời: Công nhận sự khó khăn của tình huống và nhẹ nhàng thăm dò suy nghĩ của họ trước khi đưa ra bất kỳ gợi ý nào.]\n" +
        "- Người dùng cảm thấy cô đơn -> [Gợi ý trả lời: Bày tỏ sự ấm áp và sự hiện diện, nhắc nhở họ rằng họ không cô đơn, và đề nghị được đồng hành.]\n" +
        "TUYỆT ĐỐI KHÔNG tạo ra phản hồi hoàn chỉnh cho Emo. Chỉ cung cấp hướng dẫn trong dấu ngoặc vuông.";

    public GeminiInstructionGenerator(IOptions<GeminiConfig> config, ILogger<GeminiInstructionGenerator> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GenerateInstructionAsync(string userMessage, string? history = null, string? persona = null)
    {
        try
        {
            var payload = BuildPayload(userMessage, history, persona);
            var instruction = await CallGeminiAPIAsync(payload);
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
        var contextParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(persona)) contextParts.Add($"USER PERSONA:\n{persona}");
        if (!string.IsNullOrWhiteSpace(history)) contextParts.Add($"CONVERSATION SUMMARY:\n{history}");
        contextParts.Add($"CURRENT USER MESSAGE:\n{userMessage}");

        var fullContext = string.Join("\n\n", contextParts);

        return new GeminiRequestDto(
            Contents: [new GeminiContentDto("user", [new GeminiContentPartDto(fullContext)])],
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(SystemInstruction)),
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"https://{_config.Location}-aiplatform.googleapis.com/v1/projects/{_config.ProjectId}/locations/{_config.Location}/publishers/google/models/gemini-1.5-flash-preview-0514:generateContent";

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