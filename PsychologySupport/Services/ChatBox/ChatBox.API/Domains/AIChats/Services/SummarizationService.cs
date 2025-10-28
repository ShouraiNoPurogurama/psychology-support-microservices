using System.Text;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.Gemini;
using ChatBox.API.Domains.AIChats.Events;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// THÊM CÁC USING CẦN THIẾT
using System.Net.Http; // Cho IHttpClientFactory
using Microsoft.Extensions.Logging; // Cho ILogger
using Newtonsoft.Json.Serialization; // Cho CamelCase
using System; // Cho TimeZoneInfo
using System.Collections.Generic; // Cho List
using System.Linq; // Cho LINQ

namespace ChatBox.API.Domains.AIChats.Services;

// 1. CẬP NHẬT CONSTRUCTOR
public class SummarizationService
{
    private readonly GeminiConfig _config;
    private readonly ChatBoxDbContext _dbContext;
    private readonly SessionService _sessionService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SummarizationService> _logger;
    private readonly TimeZoneInfo _vietnamTimeZone;

    public SummarizationService(
        IOptions<GeminiConfig> config,
        ChatBoxDbContext dbContext,
        SessionService sessionService,
        IHttpClientFactory httpClientFactory,
        ILogger<SummarizationService> logger)
    {
        _config = config.Value;
        _dbContext = dbContext;
        _sessionService = sessionService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // Cache lại TimeZone của VN (UTC+7)
        try
        {
            _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogWarning("Không tìm thấy TimeZone 'SE Asia Standard Time', sử dụng UTC+7 cố định.");
            _vietnamTimeZone = TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
        }
    }

    public async Task MaybeSummarizeSessionAsync(Guid userId, Guid sessionId)
    {
        var messagesCount = await _dbContext.AIChatMessages
            .CountAsync(m => m.SessionId == sessionId);

        var session = await _dbContext.AIChatSessions.FirstAsync(s => s.Id == sessionId);

        var lastIndex = session.LastSummarizedIndex ?? 0;

        var newMessagesCount = messagesCount - (lastIndex + 1);

        if (newMessagesCount > 10)
        {
            var newMessages = await _dbContext.AIChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedDate)
                .Skip(lastIndex + 1)
                .ToListAsync();

            session.AddDomainEvent(new AIChatSessionSummarizedEvent(userId, sessionId, newMessages));

            _dbContext.AIChatSessions.Update(session);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<string> CallGeminiSummarizationV1BetaAsync(List<GeminiContentDto> contents)
    {
        var apiKey = _config.ApiKey;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

        _logger.LogInformation("Gọi Gemini structured output để tóm tắt {Count} tin nhắn.", contents.Count);

        // --- BƯỚC A: ĐỊNH NGHĨA JSON SCHEMA (Structured Output) ---
        // Schema này khớp với `SummaryInstruction` từ các prompt trước
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                current = new
                {
                    type = "string",
                    description = "Tóm tắt ngắn gọn (1-2 câu) nội dung chính của 'Đoạn Hội Thoại Mới'."
                },
                persist = new
                {
                    type = "string",
                    description =
                        "Cập nhật và tổng hợp các thông tin quan trọng, bền vững (như tên, sở thích, bối cảnh cá nhân) từ 'Tóm Tắt Cũ' và 'Đoạn Hội Thoại Mới'."
                },
                metadata = new
                {
                    type = "object",
                    properties = new
                    {
                        emotionContext = new
                        {
                            type = "string",
                            description =
                                "Mô tả trạng thái cảm xúc HOẶC động lực cốt lõi của người dùng (dùng cho trí nhớ chatbot). Ví dụ: 'buồn bã vì nhớ nhà'."
                        },
                        topic = new
                        {
                            type = "string",
                            description = "Chủ đề chính của 'Đoạn Hội Thoại Mới' (ví dụ: 'hỏi thăm', 'kể chuyện công sở')."
                        },
                        imageContext = new
                        {
                            type = "string",
                            description =
                                "Một cụm mô tả NGẮN GỌN (tối đa 20 từ) mô tả CẢM XÚC + HÀNH ĐỘNG/BỐI CẢNH có thể HÌNH DUNG được (dùng cho tạo ảnh)."
                        }
                    },
                    required = new[] { "emotionContext", "topic", "imageContext" }
                }
            },
            required = new[] { "current", "persist", "metadata" }
        };

        // --- BƯỚC B: TÁI TẠO LẠI LOGIC PROMPT CŨ CỦA BẠN ---
        // (Logic cũ của bạn là build 1 string từ 'contents' và nhét vào 1 content mới)
        var promptText =
            $"Tóm tắt đoạn hội thoại sau:\n{string.Join("\n", contents.Select(c => $"{c.Role}: {c.Parts[0].Text}"))}";

        // Giả định các DTO (GeminiRequestDto, v.v.) tồn tại trong namespace Dtos.Gemini
        var payload = new GeminiStructuredOutputRequestDto(
            Contents: new List<GeminiContentDto> { new("user", [new GeminiContentPartDto(promptText)]) },
            SystemInstruction: new GeminiSystemInstructionDto(
                new GeminiContentPartDto(_config.SummaryInstruction) // Dùng instruction từ config
            ),
            GenerationConfig: new GeminiStructuredOutputGenerationConfigDto(
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

        // --- BƯỚC C: GỌI API BẰNG HTTPCLIENTFACTORY VÀ NEWTONSOFT.JSON ---
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore // Quan trọng khi gửi schema
        };

        var jsonPayload = JsonConvert.SerializeObject(payload, settings);
        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient("GeminiSummarization"); // (Nên đăng ký client này trong Program.cs)

        _logger.LogDebug("[Gemini Summary Request]: {Payload}", jsonPayload);

        var response = await client.PostAsync(url, httpContent);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API call failed: {StatusCode}\n{Result}", response.StatusCode, result);
            throw new Exception($"Gemini API call failed: {response.StatusCode}\n{result}");
        }

        // --- BƯỚC D: PARSE RESPONSE (Giống code cũ) VÀ THÊM TRƯỜNG `created_at` ---
        var parsed = JObject.Parse(result);
        var responseText = parsed["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogWarning("Gemini API trả về nội dung rỗng. Full response: {Result}", result);
            throw new Exception("Failed to summarize history: Gemini returned empty content.");
        }

        try
        {
            // Parse chuỗi JSON mà Gemini trả về
            var summaryJObject = JObject.Parse(responseText);

            // Lấy giờ VN (UTC+7)
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            // Gán thêm field "created_at" (giờ VN)
            summaryJObject["created_at"] = vietnamTime.ToString("o"); // Định dạng ISO 8601 (2025-10-27T20:50:00.000+07:00)

            // Trả về chuỗi JSON đã được cập nhật
            return summaryJObject.ToString(Formatting.None); // Formatting.None để tiết kiệm dung lượng DB
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Gemini trả về JSON không hợp lệ dù đã dùng structured output. Response: {ResponseText}",
                responseText);
            throw new InvalidOperationException("Gemini returned invalid JSON structure.", ex);
        }
    }

    // 4. GIỮ NGUYÊN METHOD NÀY
    // (Nó sẽ tự động lưu chuỗi JSON mới vào DB)
    public async Task<bool> UpdateSessionSummarizationAsync(Guid userId, Guid sessionId, string summary, int newMessageCount)
    {
        var session = await _sessionService.GetSessionAsync(userId, sessionId);
        var persona = session.PersonaSnapshot;

        if (persona is not null)
        {
            var sessionSummarizations = session.Summarization ?? "";
            var lastTwo = sessionSummarizations
                .Split("\n---\n", StringSplitOptions.RemoveEmptyEntries)
                .ToList();


            //Tạo block mới cho bản tóm tắt hiện tại (bây giờ 'summary' là một chuỗi JSON)
            lastTwo.Add(summary);

            while (lastTwo.Count > 2)
                lastTwo.RemoveAt(0);

            session.Summarization = string.Join("\n---\n", lastTwo);
        }
        else
        {
            //Nếu không có persona, chỉ lưu tóm tắt như bình thường
            var sessionSummarizations = session.Summarization ?? "";
            var lastTwo = sessionSummarizations
                .Split("\n---\n", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            lastTwo.Add(summary);
            while (lastTwo.Count > 2)
                lastTwo.RemoveAt(0);

            session.Summarization = string.Join("\n---\n", lastTwo);
        }

        session.LastSummarizedAt = DateTimeOffset.UtcNow;
        session.LastSummarizedIndex = (session.LastSummarizedIndex ?? 0) + newMessageCount;

        await _sessionService.UpdateSessionAsync(session);

        return true;
    }
}