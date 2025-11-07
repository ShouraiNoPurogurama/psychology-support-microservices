using System.Text;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Enums;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChatBox.API.Domains.AIChats.Services;

public class GeminiRouterClient : IRouterClient
{
    private readonly ILogger<GeminiRouterClient> _logger;
    private readonly IConfiguration _cfg;
    private readonly GeminiConfig _config;

    public GeminiRouterClient(
        ILogger<GeminiRouterClient> logger,
        IConfiguration cfg,
        IOptions<GeminiConfig> config)
    {
        _logger = logger;
        _cfg = cfg;
        _config = config.Value;
    }

    public async Task<RouterDecisionDto?> RouteAsync(string userMessage, List<HistoryMessage> historyMessages,
        CancellationToken ct = default)
    {
        try
        {
            var payload = BuildPayload(userMessage, historyMessages);

            _logger.LogInformation("Router Gemini payload: {Payload}", JsonConvert.SerializeObject(payload));

            var json = await CallGeminiStructuredOutputAPIAsync(payload, ct);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            var decision = JsonConvert.DeserializeObject<RouterDecisionDto>(json);
            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Router call failed");
            return null;
        }
    }

    private GeminiStructuredOutputRequestDto BuildPayload(string userMessage, List<HistoryMessage> historyMessages)
    {
        var systemInstruction = BuildRouterSystemInstruction();

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

        var responseSchema = BuildRouterResponseSchema();

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

    private object BuildRouterResponseSchema()
    {
        var emotionEnums = Enum.GetNames(typeof(EmotionTag));
        var relationshipEnums = Enum.GetNames(typeof(RelationshipTag));
        var topicEnums = Enum.GetNames(typeof(TopicTag));
        var intentEnums = Enum.GetNames(typeof(RouterIntent)); 
        var toolTypeEnums = Enum.GetNames(typeof(RouterToolType)); // ví dụ: DASS21_TEST
        var combinedEnums = topicEnums.Concat(emotionEnums).Concat(relationshipEnums).ToArray();

        return new
        {
            type = "object",
            description = "Router → Kết quả structured: route | guidance | retrieval | memory | tool_call",
            properties = new
            {
                // 1) Quyết định định tuyến (bắt buộc)
                route = new
                {
                    type = "object",
                    description =
                        "Quyết định định tuyến cấp 1.\n" +
                        "- CONVERSATION: trò chuyện/đáp ngắn mang tính kết nối.\n" +
                        "- SAFETY_REFUSAL: từ chối an toàn.\n" +
                        "- RAG_PERSONAL_MEMORY: cần ký ức cá nhân để cá nhân hoá.\n" +
                        "- RAG_TEAM_KNOWLEDGE: hỏi về chi tiết nội bộ (Emo/EmoEase/Soltech/FPTU/chính sách...).\n" +
                        "- TOOL_CALLING: cần gọi công cụ (VD: test DASS-21).",
                    properties = new
                    {
                        intent = new
                        {
                            type = "string",
                            description =
                                "Chọn 1 intent định tuyến: CONVERSATION | SAFETY_REFUSAL | RAG_PERSONAL_MEMORY | RAG_TEAM_KNOWLEDGE | TOOL_CALLING.",
                            @enum = intentEnums
                        }
                    },
                    required = new[] { "intent" }
                },

                // 2) Guidance cho Emo (bắt buộc)
                guidance = new
                {
                    type = "object",
                    description =
                        "Chỉ dẫn 1 dòng (KHÔNG phải câu trả lời), để Emo giữ đúng vai và nhịp.\n" +
                        "Dùng đúng dấu ngoặc vuông. Tránh hỏi dồn; tối đa 1 câu hỏi mở ngắn; có thể kết thúc mà không hỏi.\n" +
                        "Nếu intent = RAG_TEAM_KNOWLEDGE, chèn marker: [MARKER: RAG_TEAM_KNOWLEDGE].",
                    properties = new
                    {
                        emo_instruction = new
                        {
                            type = "string",
                            description =
                                "Một dòng duy nhất trong ngoặc vuông, nêu HÀNH ĐỘNG/HƯỚNG DẪN cho Emo (nên nói gì, lưu ý gì, giọng điệu ra sao). " +
                                "KHÔNG viết câu trả lời mẫu. Ví dụ: [Gợi ý trả lời: Xác nhận cảm xúc mệt mỏi và mời họ kể thêm một chút nếu sẵn sàng.]"
                        }
                    },
                    required = new[] { "emo_instruction" }
                },

                // 3) Retrieval flags (optional; có required nội bộ)
                retrieval = new
                {
                    type = "object",
                    description =
                        "Quyết định augment context bằng ký ức cá nhân hoặc knowledge nội bộ.\n" +
                        "- Đặt needed = true nếu CẦN cá nhân hoá hoặc kiến thức nội bộ.\n" +
                        "- Lưu ý: Router “đọc theo chuỗi” 3–4 tin nhắn liền kề (đại từ, phủ định, đính chính).",
                    properties = new
                    {
                        needed = new
                        {
                            type = "boolean",
                            description =
                                "TRUE nếu: câu hỏi gợi ý/đề xuất có ràng buộc cá nhân (sở thích, thời gian, ngân sách...), " +
                                "cần nhớ lại bối cảnh cũ hoặc hỏi kiến thức nội bộ. FALSE nếu small talk/đùa ngắn/fact hiển nhiên."
                        },
                        scopes = new
                        {
                            type = "object",
                            description =
                                "Phạm vi retrieval. Bật những phạm vi phù hợp với intent.\n" +
                                "- personal_memory: RAG ký ức cá nhân.\n" +
                                "- team_knowledge: kiến thức nội bộ dự án/đội ngũ.",
                            properties = new
                            {
                                personal_memory = new { type = "boolean", description = "Dùng RAG ký ức cá nhân." },
                                team_knowledge = new { type = "boolean", description = "Dùng kiến thức nội bộ EmoEase." }
                            },
                            required = new[] { "personal_memory", "team_knowledge" }
                        },
                        hints = new
                        {
                            type = "object",
                            description = "Gợi ý từ khoá (optional) để BE tăng chất lượng truy hồi.",
                            properties = new
                            {
                                keywords = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            }
                        }
                    }
                },

                // 4) Memory save (optional; có required nội bộ)
                memory = new
                {
                    type = "object",
                    description =
                        "Khối nhớ – dùng khi user đưa fact bền hoặc phủ định/sửa fact cũ.\n" +
                        "Nếu save.needed = true thì BẮT BUỘC có payload.summary.",
                    properties = new
                    {
                        save = new
                        {
                            type = "object",
                            description = "Cấu hình lưu ký ức.",
                            properties = new
                            {
                                needed = new
                                {
                                    type = "boolean",
                                    description =
                                        "TRUE khi: user cung cấp fact bền (sở thích/không thích/dị ứng/mục tiêu/sự kiện...) " +
                                        "hoặc user đính chính ký ức cũ."
                                },
                                payload = new
                                {
                                    type = "object",
                                    description = "Chi tiết memory cần lưu (nếu needed = true).",
                                    properties = new
                                    {
                                        summary = new
                                        {
                                            type = "string",
                                            description =
                                                "BẮT BUỘC bắt đầu bằng một trong: 'Sở thích:' | 'Không thích:' | 'Dị ứng:' | 'Fact:' | 'Mục tiêu:' | 'Sự kiện:'. " +
                                                "Viết 1–2 câu **bám sát lời user** và **canonical hoá** để dễ truy hồi (đối tượng + domain/app + aka nếu có). " +
                                                "Không dùng dấu ngoặc kép bao quanh cụm."
                                        },
                                        emotion_tags = new
                                        {
                                            type = "array",
                                            description = "Các cảm xúc liên quan.",
                                            items = new { type = "string", @enum = emotionEnums }
                                        },
                                        relationship_tags = new
                                        {
                                            type = "array",
                                            description = "Các quan hệ/đối tượng liên quan.",
                                            items = new { type = "string", @enum = relationshipEnums }
                                        },
                                        topic_tags = new
                                        {
                                            type = "array",
                                            description = "Các chủ đề liên quan.",
                                            items = new { type = "string", @enum = topicEnums }
                                        },
                                        normalized_tags = new
                                        {
                                            type = "array",
                                            description =
                                                "Enum hợp nhất (Topic_*, Emotion_*, Relationship_*) để truy hồi thống nhất.",
                                            items = new { type = "string", @enum = combinedEnums }
                                        }
                                    },
                                    required = new[] { "summary" }
                                }
                            },
                            required = new[] { "needed" }
                        }
                    }
                },

                // 5) Tool calling (optional; chứa CTA bên trong; có required nội bộ)
                tool_call = new
                {
                    type = "object",
                    description =
                        "Chỉ xuất khi route.intent = TOOL_CALLING.\n" +
                        "- Backend sẽ resolve resourceKey → URL/metadata thật.\n" +
                        "- Ví dụ: type = DASS21_TEST, resourceKey = 'DASS21_FE_LINK'.\n" +
                        "- CTA được NHÚNG BÊN TRONG tool_call (chỉ hiện khi cần hành động rõ ràng cho người dùng).",
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
                        },

                        // 👉 CTA nested tại đây
                        cta = new
                        {
                            type = "object",
                            description =
                                "Khối CTA để FE render nút khi tool cần tương tác người dùng.\n" +
                                "Ví dụ DASS-21: title='Bạn muốn làm bài DASS-21 chứ?' với 2 nút 'Có'/'Không'.",
                            properties = new
                            {
                                needed = new { type = "boolean", description = "TRUE nếu cần hiển thị CTA." },
                                title = new { type = "string", description = "Tiêu đề CTA ngắn gọn." },
                                resourceKey = new
                                {
                                    type = "string",
                                    description = "Nếu CTA gắn với tài nguyên chuẩn (VD: 'DASS21_FE_LINK')."
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
                                                description = "BE điền khi action=NAVIGATE (VD: link test DASS-21)."
                                            }
                                        },
                                        required = new[] { "label", "action" }
                                    }
                                }
                            }
                            // cta không bắt buộc; nhưng nếu đã có, FE/BE có thể kiểm tra 'needed'
                        }
                    },
                    required = new[] { "needed", "type" } // khi đã xuất block tool_call, 2 field này phải có
                }
            },

            // Top-level: chỉ bắt buộc 2 khối lõi
            required = new[] { "route", "guidance" }
        };
    }


    private string BuildRouterSystemInstruction()
    {
        return """
               Bạn là ROUTER-RESPONDER cho Emo để phản hồi cho [USER MESSAGE] một cách đúng trọng tâm, chân thành. Xuất **một JSON object duy nhất** đúng ResponseSchema. Không thêm chữ, không markdown.
               """;
    }

    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload, CancellationToken ct)
    {
        using var http = new HttpClient();

        var apiKey = _cfg["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload, settings), Encoding.UTF8, "application/json");
        var resp = await http.PostAsync(url, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Router Gemini call failed: {StatusCode} - {Body}", resp.StatusCode, body);
            return string.Empty;
        }

        var jo = JObject.Parse(body);
        var text = jo["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "";
        return text.Trim();
    }

    private async Task<string> CallGeminiStructuredOutputAPIAsync(GeminiStructuredOutputRequestDto payload, CancellationToken ct)
    {
        using var http = new HttpClient();

        var apiKey = _cfg["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync(url, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Router Gemini call failed: {StatusCode} - {Body}", resp.StatusCode, body);
            return string.Empty;
        }

        _logger.LogInformation("Router Gemini response: {Body}", body);

        var jo = JObject.Parse(body);
        var text = jo["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "";
        return text.Trim();
    }
}