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
        // === System prompt dành cho ROUTER (chuẩn) ===
        // * BẮT BUỘC * trả về 1 JSON object DUY NHẤT theo schema — không thêm text.
        var systemInstruction = BuildRouterSystemInstruction();

        // context đầu vào cho model
        var contextParts = new List<string>();
        // if (!string.IsNullOrWhiteSpace(history))  contextParts.Add($"[CONVERSATION SUMMARY]\n{history}");
        
        if(historyMessages.Count > 0) 
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

        // === JSON Schema cho Structured Output ===
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
        var combinedEnums = topicEnums.Concat(emotionEnums).Concat(relationshipEnums).ToArray();

        return new
        {
            type = "object",
            properties = new
            {
                intent = new
                {
                    type = "string",
                    description = "Chọn 1 intent định tuyến: DIRECT_ANSWER, SMALL_TALK, SAFETY_REFUSAL (trả lời trực tiếp/từ chối an toàn); RAG_PERSONAL_MEMORY (cần ký ức cá nhân để gợi ý/cá nhân hoá); RAG_TEAM_KNOWLEDGE (hỏi về chi tiết doanh nghiệp/dự án Emo/EmoEase/Soltech/FPTU/chính sách).",
                    @enum = intentEnums
                },
                emo_instruction = new
                {
                    type = "string",
                    description = "Một dòng duy nhất trong ngoặc vuông, nêu HÀNH ĐỘNG/HƯỚNG DẪN cho Emo (nên nói gì, lưu ý gì, giọng điệu ra sao). Tránh hỏi dồn; chỉ gợi mở tối đa 1 câu hỏi ngắn nếu người dùng tỏ ý muốn kể thêm; có thể kết thúc mà không hỏi. KHÔNG viết câu trả lời mẫu. Nếu intent là RAG_TEAM_KNOWLEDGE, dùng marker: [MARKER: RAG_TEAM_KNOWLEDGE]."
                },
                save_needed = new
                {
                    type = "boolean",
                    description = "Đặt 'true' khi user cung cấp fact mới bền vững (sở thích, sự kiện, mục tiêu) hoặc user phủ định/sửa ký ức cũ."
                },
                retrieval_needed = new
                {
                    type = "boolean",
                    description = "Đặt 'true' khi câu trả lời CẦN cá nhân hoá hoặc kiến thức nội bộ. " +
        "Router nhận 3–4 tin nhắn liền kề nên phải xét **ngữ nghĩa chuỗi** (follow-up, đại từ, phủ định, đính chính, tham chiếu ngầm) trước khi quyết định.\n" +
        "TRUE nếu: câu hỏi gợi ý/đề xuất (ăn, học, xem...), có thể có ràng buộc cá nhân (sở thích, thời gian, ngân sách...), cần nhớ lại hoặc hỏi kiến thức nội bộ.\n" +
        "FALSE nếu: small talk, đùa ngắn, fact hiển nhiên không cần truy xuất lại kí ức về người dùng."
                },
                memory_to_save = new
                {
                    type = "object",
                    description = "BẮT BUỘC nếu 'save_needed' = true.",
                    properties = new
                    {
                        summary = new
                        {
                            type = "string",
                            description =
                                "BẮT BUỘC bắt đầu bằng một trong các tiền tố: 'Sở thích:', 'Không thích:', 'Dị ứng:', 'Fact:', 'Mục tiêu:', 'Sự kiện'. Viết 1–2 câu **bám sát lời user** nhưng **bổ sung ngữ cảnh/canonical** để dễ truy hồi: nêu LOẠI ĐỐI TƯỢNG + DOMAIN/APP (game/app/địa điểm/thể loại) + AKA nếu có. **Không dùng dấu ngoặc kép** quanh từ/cụm. Ví dụ: 'Sở thích: Chơi tướng Yasuo trong game Liên Minh Huyền Thoại (League of Legends).'"
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
                            description = "Các quan hệ hoặc đối tượng liên quan.",
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
                            description = "Enum hợp nhất (Topic_*, Emotion_*, Relationship_*) — để truy hồi thống nhất.",
                            items = new { type = "string", @enum = combinedEnums }
                        }
                    },
                    required = new[] { "summary" } 
                }
            },
            required = new[] { "intent", "emo_instruction", "save_needed", "retrieval_needed" }
        };
    }


    private string BuildRouterSystemInstruction()
    {
//         return """
//                Bạn là ROUTER-RESPONDER cho Emo. Xuất **một JSON object duy nhất** đúng ResponseSchema. Không thêm chữ, không markdown.
//
//                # Examples
//                [User]: tui thích chơi Yasuo
//                {"intent":"DIRECT_ANSWER","emo_instruction":"[Gợi ý trả lời: Công nhận sở thích và niềm vui khi chơi Yasuo; giữ giọng điệu hào hứng, có thể mời gọi nhẹ nếu họ muốn kể thêm về cách chơi.]", "save_needed":true, "memory_to_save":{"summary":"Sở thích: Chơi tướng Yasuo trong game Liên Minh Huyền Thoại (League of Legends).","normalized_tags":["Topic_Hobby"]}, "retrieval_needed":false}
//
//                [User]: giờ đánh liên minh chơi tướng nào cho vui?
//                {"intent":"RAG_PERSONAL_MEMORY","emo_instruction":"[Gợi ý trả lời: Dựa trên tướng ưa thích trước đây để gợi ý vài lựa chọn cùng phong cách; ưu tiên mô tả cảm giác chơi, không cần hỏi dồn – có thể gợi ý nhẹ nếu họ muốn.]", "save_needed":false, "retrieval_needed":true}
//
//                [User]: EmoEase do ai làm?
//                {"intent":"RAG_TEAM_KNOWLEDGE","emo_instruction":"[MARKER: RAG_TEAM_KNOWLEDGE]", "save_needed":false, "retrieval_needed":true}
//
//                [User]: đố mày tao mới kiểm tra được mấy điểm
                  // {"intent":"RAG_PERSONAL_MEMORY","emo_instruction":"[Gợi ý trả lời: Giữ tông vui và tôn trọng; nếu có ký ức liên quan thì phản ánh tinh tế; tránh đoán chắc; có thể mời họ chia sẻ nếu muốn khoe kết quả hoặc quá trình để đạt được kết quả.]", "save_needed":false, "retrieval_needed":true}
//                """;
        
        return """
               Bạn là ROUTER-RESPONDER cho Emo để phản hồi cho [USER MESSAGE] một cách đúng trọng tâm, chân thành. Xuất **một JSON object duy nhất** đúng ResponseSchema. Không thêm chữ, không markdown.

               #Ví dụ:
               [USER MESSAGE]: đố mày tao mới kiểm tra được mấy điểm => các flag sẽ là "intent":"RAG_PERSONAL_MEMORY", "save_needed":false, "retrieval_needed":true
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