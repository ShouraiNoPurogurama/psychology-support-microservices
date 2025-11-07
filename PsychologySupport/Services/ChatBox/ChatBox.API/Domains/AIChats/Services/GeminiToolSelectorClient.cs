
using System.Text;
using AutoGen.Gemini;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.AI.Specialist;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Enums;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiToolSelectorClient : IToolSelectorClient
{
    private readonly ILogger<GeminiToolSelectorClient> _logger;
    private readonly IAIProvider _aiProvider;
    private readonly IConfiguration _cfg;
    private readonly GeminiConfig _config;

    public GeminiToolSelectorClient(
        ILogger<GeminiToolSelectorClient> logger,
        IConfiguration cfg,
        IOptions<GeminiConfig> config)
    {
        _logger = logger;
        _cfg = cfg;
        _config = config.Value;
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

            // Deserialize vào DTO mới (ToolSelectorResultDto)
            var decision = JsonConvert.DeserializeObject<ToolSelectorResultDto>(json);
            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ToolSelector call failed");
            return null;
        }
    }

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

        var responseSchema = BuildToolSelectorSchema(); // Schema "nặng" nằm ở đây

        return new GeminiStructuredOutputRequestDto(
            Contents: [new GeminiContentDto("user", [new GeminiContentPartDto(fullContext)])],
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(systemInstruction)),
            GenerationConfig: new GeminiStructuredOutputGenerationConfigDto(
                ResponseMimeType: "application/json",
                ResponseSchema: responseSchema
            ),
            SafetySettings: [ /* ... (như cũ) ... */ ]
        );
    }

    private string BuildSystemInstruction()
    {
        return """
               Bạn là TOOL-SELECTOR (Bộ chọn công cụ). Nhiệm vụ DUY NHẤT của bạn là phân tích [USER MESSAGE] và quyết định xem họ có cần một "công cụ" (như làm bài test, nghe nhạc) hay không.
               Xuất **một JSON object duy nhất** đúng ResponseSchema. Không thêm chữ, không markdown.
               Tập trung vào việc chọn đúng 'toolCall.type' và 'cta' nếu cần.
               """;
    }

    // HÀM QUAN TRỌNG NHẤT: Build schema từ DTO có sẵn của mày
    private object BuildToolSelectorSchema()
    {
        var toolTypeEnums = Enum.GetNames(typeof(RouterToolType)); // DASS21_TEST, PLAY_EMOBALANCE_MUSIC

        return new
        {
            type = "object",
            description = "Quyết định tool và CTA (nếu có).",
            properties = new
            {
                // Property 1: "toolCall" (dựa trên ToolCallBlock.cs)
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
                            description = "Loại tool cần gọi (enum RouterToolType).",
                            @enum = toolTypeEnums
                        },
                        resourceKey = new
                        {
                            type = "string",
                            description = "Khoá tài nguyên chuẩn trong BE (VD: 'DASS21_FE_LINK')."
                        },
                        hints = new
                        {
                            type = "object",
                            description = "Gợi ý mờ cho BE (optional).",
                            additionalProperties = true
                        }
                    },
                    required = new[] { "needed", "type" } // Khi đã xuất, 2 field này phải có
                },

                // Property 2: "cta" (dựa trên CtaBlock.cs, CtaButton.cs)
                cta = new
                {
                    type = "object",
                    description = "Khối CTA để FE render nút (optional).",
                    properties = new
                    {
                        needed = new { type = "boolean", description = "TRUE nếu cần hiển thị CTA." },
                        title = new { type = "string", description = "Tiêu đề CTA ngắn gọn." },
                        resourceKey = new
                        {
                            type = "string",
                            description = "Nếu CTA gắn với tài nguyên chuẩn."
                        },
                        buttons = new
                        {
                            type = "array",
                            description = "Danh sách nút để FE render.",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    label = new { type = "string", description = "Văn bản nút (VD: 'Có', 'Không')." },
                                    action = new { type = "string", description = "NAVIGATE | DISMISS" },
                                    url = new
                                    {
                                        type = "string",
                                        nullable = true,
                                        description = "BE điền khi action=NAVIGATE."
                                    }
                                },
                                required = new[] { "label", "action" }
                            }
                        }
                    }
                }
            },
            required = new[] { "toolCall" } // Luôn phải trả về toolCall (dù needed=false)
        };
    }
}