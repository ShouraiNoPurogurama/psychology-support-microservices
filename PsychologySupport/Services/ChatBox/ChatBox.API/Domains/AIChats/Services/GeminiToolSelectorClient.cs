using System.Text;
using ChatBox.API.Domains.AIChats.Dtos.AI;
// Sửa using DTO
using ChatBox.API.Domains.AIChats.Dtos.AI.Router;
using ChatBox.API.Domains.AIChats.Dtos.AI.Specialist;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Enums;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiToolSelectorClient : IToolSelectorClient
{
    private readonly ILogger<GeminiToolSelectorClient> _logger;
    private readonly IAIProvider _aiProvider;
    private readonly IConfiguration _cfg;

    public GeminiToolSelectorClient(
        ILogger<GeminiToolSelectorClient> logger,
        IAIProvider aiProvider,
        IConfiguration cfg
    )
    {
        _logger = logger;
        _aiProvider = aiProvider;
        _cfg = cfg;
    }

    public async Task<ToolSelectorResultDto?> SelectToolAsync(string userMessage, List<HistoryMessage> historyMessages,
        CancellationToken ct = default)
    {
        try
        {
            var payload = BuildPayload(userMessage, historyMessages);
            _logger.LogInformation("ToolSelector Gemini payload: {Payload}", JsonConvert.SerializeObject(payload));

            var json = await _aiProvider.CallGeminiStructuredOutputAPIAsync(payload, ct);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var decision = JsonConvert.DeserializeObject<ToolSelectorResultDto>(json);

            if (!string.IsNullOrWhiteSpace(decision?.Cta?.NavigateUrl))
            {
                _logger.LogWarning("ToolSelector generated URL: {Url}", decision.Cta.NavigateUrl);
            }

            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ToolSelector call failed");
            return null;
        }
    }

    // ... (Hàm BuildPayload giữ nguyên) ...
    private GeminiStructuredOutputRequestDto BuildPayload(string userMessage, List<HistoryMessage> historyMessages)
    {
        var systemInstruction = BuildSystemInstruction();

        var contextParts = new List<string>();
        if (historyMessages.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[CONVERSATION HISTORY]:");
            foreach (var msg in historyMessages.TakeLast(3))
            {
                var speaker = msg.IsFromAI ? "Emo" : "User";
                sb.AppendLine($"{speaker}: {msg.Content}");
            }

            contextParts.Add(sb.ToString().Trim());
        }

        contextParts.Add($"\n\n[USER MESSAGE]: \n {userMessage}");
        var fullContext = string.Join("\n\n", contextParts);

        var responseSchema = BuildToolSelectorSchema();

        return new GeminiStructuredOutputRequestDto(
            Contents: [new GeminiContentDto("user", [new GeminiContentPartDto(fullContext)])],
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(systemInstruction)),
            GenerationConfig: new GeminiStructuredOutputGenerationConfigDto(
                ResponseMimeType: "application/json",
                ResponseSchema: responseSchema
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

    private string BuildSystemInstruction()
    {
        var baseUrl = _cfg["App:WebAppBaseUrl"] ?? "https://www.emoease.vn";

        return $"""
                Bạn là TOOL-SELECTOR (Bộ chọn công cụ). Nhiệm vụ DUY NHẤT của bạn là phân tích [USER MESSAGE] và quyết định xem họ có cần một "công cụ" (như làm bài test, nghe nhạc) hay không.
                Xuất **một JSON object duy nhất** đúng ResponseSchema. Không thêm chữ, không markdown.

                [KNOWLEDGE MAPPING]
                - DASS21_TEST (resourceKey 'DASS21_FE_LINK'): {baseUrl}/tests/dass-21
                - PLAY_EMOBALANCE_MUSIC (resourceKey 'EMOBALANCE_MUSIC_PLAYER'): {baseUrl}/music/emobalance

                [LOGICAL RULES]
                1. 'toolCall.needed' và 'cta.needed' PHẢI có cùng giá trị (cả hai cùng true, hoặc cả hai cùng false).
                2. Nếu 'needed' = true: 'toolCall.type', 'cta.title', và 'cta.navigateUrl' PHẢI khớp với NHAU (cùng 1 tool từ [KNOWLEDGE MAPPING]).
                3. TUYỆT ĐỐI KHÔNG hỏi chung chung (VD: 'test tính cách hay tâm trạng?'). Chọn 1 tool CỤ THỂ.

                [FORMATTING RULES]
                - 'cta.navigateUrl': PHẢI là URL đầy đủ từ [KNOWLEDGE MAPPING].
                """;
    }

    private object BuildToolSelectorSchema()
{
    var toolTypeEnums = Enum.GetNames(typeof(RouterToolType));
    var baseUrl = _cfg["App:WebAppBaseUrl"] ?? "https://www.emoease.vn";

    string knowledgeMapping = $"""
        [KNOWLEDGE MAPPING]:
        - DASS21_TEST (resourceKey 'DASS21_FE_LINK'): {baseUrl}/tests/dass-21
        - PLAY_EMOBALANCE_MUSIC (resourceKey 'EMOBALANCE_MUSIC_PLAYER'): {baseUrl}/music/emobalance
        """;

    return new
    {
        type = "object",
        description = $"Quyết định tool và CTA (nếu có). {knowledgeMapping}",
        properties = new
        {
            toolCall = new
            {
                type = "object",
                description = "Quyết định tool chính.",
                properties = new
                {
                    needed = new { type = "boolean", description = "TRUE khi cần gọi tool." },
                    type = new
                    {
                        type = "string",
                        description = "Loại tool cần gọi (enum RouterToolType). Phải là một trong [KNOWLEDGE MAPPING].",
                        @enum = toolTypeEnums
                    },
                    resourceKey = new
                    {
                        type = "string",
                        description = "Khoá tài nguyên CHUẨN từ [KNOWLEDGE MAPPING] (VD: 'DASS21_FE_LINK')."
                    }
                },
                required = new[] { "needed", "type" }
            },

            cta = new
            {
                type = "object",
                description = "Khối CTA (FE sẽ tự render nút 'Từ chối').",
                properties = new
                {
                    needed = new { type = "boolean", description = "TRUE nếu cần hiển thị CTA." },
                    title = new
                    {
                        type = "string",
                        description = "Nhiệm vụ: Viết một câu hỏi, thân thiện gợi mở user sử dụng công cụ được gọi. Định dạng: Chỉ văn bản thuần tuý (plain text), không chứa HTML, <br/>, <a>, hay URL."
                    },
                    resourceKey = new
                    {
                        type = "string",
                        description = "Khoá tài nguyên CHUẨN, giống 'toolCall.resourceKey'."
                    },
                    navigateUrl = new
                    {
                        type = "string",
                        nullable = true,
                        description = $"Nhiệm vụ: Điền URL đầy đủ từ [KNOWLEDGE MAPPING] nếu 'needed' = true. (VD: '{baseUrl}/tests/dass-21')."
                    }
                },
                required = new[] {"needed", "title" ,"navigateUrl", "resourceKey" }
            }
        },
        required = new[] { "toolCall" }
    };
}
}