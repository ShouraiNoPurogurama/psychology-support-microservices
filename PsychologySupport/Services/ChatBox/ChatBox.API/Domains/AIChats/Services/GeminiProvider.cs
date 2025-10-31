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
    IContextBuilder contextBuilder,
    IConfiguration cfg)
    : IAIProvider
{
    // ===== Default: gọi trained model qua endpoint (Vertex AI) =====
    public async Task<string> GenerateResponseAsync(AIRequestPayload payload, Guid sessionId)
    {
        var geminiPayload = await BuildGeminiPayload(payload, sessionId);
        return await CallGeminiAPIAsync(geminiPayload);
    }

    // ===== Overload: gọi Gemini 2.5 Flash Lite bằng API key (non-structured) =====
    public async Task<string> GenerateResponseAsync_FoundationalModel(AIRequestPayload payload, Guid sessionId,
        string? purpose = null,
        CancellationToken ct = default)
    {
        GeminiRequestDto geminiPayload;

        if (purpose == "image_sticker_followup")
        {
            geminiPayload = BuildGeminiPayloadForImageFollowup(payload, sessionId);
        }
        else
        {
            geminiPayload = await BuildGeminiPayload(payload, sessionId);
        }

        return await CallGeminiFlashLiteAPIAsync(geminiPayload, ct);
    }

    // ===== Build chung payload =====
    private async Task<GeminiRequestDto> BuildGeminiPayload(AIRequestPayload payload, Guid sessionId)
    {
        var contents = new List<GeminiContentDto>();
        // var lastMessageBlock = await contextBuilder.GetLastEmoMessageBlock(sessionId);

        // Add summarization if exists
        if (!string.IsNullOrWhiteSpace(payload.Summarization))
        {
            contents.Add(new GeminiContentDto("user",
                [new GeminiContentPartDto($"{payload.Summarization}")]));
        }

        contents.AddRange(payload.HistoryMessages.Select(h =>
            new GeminiContentDto(h.IsFromAI ? "model" : "user",
                [new GeminiContentPartDto(h.Content)])));

        // Add current context
        contents.Add(new GeminiContentDto("user",
            [new GeminiContentPartDto(payload.Context)]));

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(config.Value.SystemInstruction)),
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
    }


    // ===== Build chung payload =====
    private GeminiRequestDto BuildGeminiPayloadForImageFollowup(AIRequestPayload payload, Guid sessionId)
    {
        var contents = new List<GeminiContentDto>();
        // var lastMessageBlock = await contextBuilder.GetLastEmoMessageBlock(sessionId);

        // Add summarization if exists
        if (!string.IsNullOrWhiteSpace(payload.Summarization))
        {
            contents.Add(new GeminiContentDto("user",
                [new GeminiContentPartDto($"{payload.Summarization}")]));
        }

        // Add current context
        contents.Add(new GeminiContentDto("user",
            [new GeminiContentPartDto(payload.Context)]));

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(
                "Bạn là Emo – bạn đồng hành thân thiện.\n    Nhiệm vụ: viết 1–2 câu ngắn (≤40 từ, tiếng Việt) để tặng sticker dựa trên STICKER_BRIEF và ngữ cảnh gần nhất.\n    Giọng cậu–tớ, ấm, tự nhiên, chân thành. Chỉ trả về thuần văn bản: không markdown, không liệt kê, không emoji, không dạy đời, không hứa hẹn “chữa lành”.\n    Không bịa chi tiết ngoài brief. Ưu tiên gợi tả hình ảnh hoặc ý nghĩa ngắn gọn liên quan chủ đề sticker và cảm xúc vừa chia sẻ.\n    Kết thúc có thể gợi mở 1 câu hỏi rất nhẹ để cậu phản hồi (ví dụ: “cậu thấy chi tiết này có đúng tâm trạng không?”).\n    Nếu gặp nội dung nhạy cảm (tự hại/bạo lực/lạm dụng), chỉ bày tỏ quan tâm ngắn gọn và khuyến khích tìm hỗ trợ tin cậy; tuyệt đối không hướng dẫn hay chẩn đoán.")),
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
    }

    // ===== Gọi model qua Vertex AI (service account token) =====
    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
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

    // ===== Gọi model Gemini 2.5 Flash Lite qua API Key (generateContent) =====
    private async Task<string> CallGeminiFlashLiteAPIAsync(GeminiRequestDto payload, CancellationToken ct)
    {
        using var http = new HttpClient();

        var apiKey = cfg["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        // var settings = new JsonSerializerSettings
        // {
        //     ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        // };

        var serializedPayload = JsonConvert.SerializeObject(payload);

        logger.LogInformation("Gemini Flash Lite Payload: {Payload}", serializedPayload);

        var content = new StringContent(serializedPayload, Encoding.UTF8, "application/json");
        var resp = await http.PostAsync(url, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("Gemini Flash Lite call failed: {StatusCode} - {Body}", resp.StatusCode, body);
            return string.Empty;
        }

        var jo = JObject.Parse(body);
        var text = jo["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "";
        return text.Trim();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}