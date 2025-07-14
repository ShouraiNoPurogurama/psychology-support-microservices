using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using BuildingBlocks.Pagination;
using ChatBox.API.Data;
using ChatBox.API.Dtos;
using ChatBox.API.Dtos.Gemini;
using ChatBox.API.Models;
using ChatBox.API.Utils;
using Google.Apis.Auth.OAuth2;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ChatBox.API.Services;

public class GeminiService(
    IOptions<GeminiConfig> config,
    ChatBoxDbContext dbContext,
    SummarizationService summarizationService,
    ILogger<GeminiService> logger)
{
    private readonly GeminiConfig _config = config.Value;

    //Sử dụng ConcurrentDictionary để thread-safe
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> SessionLocks = new();

    //Tracking active sessions để cleanup
    private static readonly ConcurrentDictionary<Guid, DateTime> ActiveSessions = new();
    
    
    private static readonly ConcurrentDictionary<Guid, Queue<string>> PendingMessagesBySession = new();


    //Cleanup timer
    private static readonly Timer CleanupTimer =
        new(CleanupInactiveSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

    const int MaxUserInputLength = 1000;
    const int SessionTimeoutMinutes = 30;//Timeout cho session không hoạt động

    public async Task<List<AIMessageResponseDto>> SendMessageAsync(AIMessageRequestDto request, Guid userId)
    {
        await ValidateSessionOwnershipAsync(request.SessionId, userId);

        var sessionLock = SessionLocks.GetOrAdd(request.SessionId, _ => new SemaphoreSlim(1, 1));

        ActiveSessions.AddOrUpdate(request.SessionId, DateTime.UtcNow, (key, old) => DateTime.UtcNow);

        var inputNormalized = request.UserMessage.Trim().ToLowerInvariant() ?? "";
        var pendingQueue = PendingMessagesBySession.GetOrAdd(request.SessionId, _ => new Queue<string>());
        
        lock (pendingQueue)//Đảm bảo thread-safe cho queue
        {
            if (pendingQueue.Count >= 2)
            {
                return new List<AIMessageResponseDto>
                {
                    new AIMessageResponseDto(
                        request.SessionId,
                        true,
                        "Cậu gửi hơi nhiều tin liên tiếp rồi nè, đợi tớ phản hồi xong hãy gửi tiếp nhé! 🕐",
                        DateTime.UtcNow)
                };
            }

            //Nếu cùng nội dung và đã có 1 message đó trong queue, cũng trả về response mẫu
            if (pendingQueue.Contains(inputNormalized))
            {
                return new List<AIMessageResponseDto>
                {
                    new AIMessageResponseDto(
                        request.SessionId,
                        true,
                        "Tớ đang xử lý tin nhắn trước của cậu rồi nè. Đợi một xíu tớ phản hồi xong sẽ trả lời tiếp nhé! 🕐",
                        DateTime.UtcNow)
                };
            }

            pendingQueue.Enqueue(inputNormalized);
        }
        
        //Timeout cho lock để tránh deadlock
        var lockAcquired = await sessionLock.WaitAsync(TimeSpan.FromSeconds(15));
        if (!lockAcquired)
        {
            logger.LogWarning("Failed to acquire lock for session {SessionId} within timeout", request.SessionId);
            //Nhớ dequeue khỏi queue nếu không acquire được lock
            lock (pendingQueue)
            {
                if (pendingQueue.Count > 0)
                    pendingQueue.Dequeue();
            }
            throw new TimeoutException("Hệ thống đang xử lý tin nhắn khác. Vui lòng thử lại.");
        }

        try
        {
            logger.LogInformation("Processing message for session {SessionId}", request.SessionId);

            var session = await dbContext.AIChatSessions
                .AsNoTracking()
                .FirstAsync(s => s.Id == request.SessionId);

            var contentParts = await LoadSessionHistoryMessages(request, session);

            var userMessageWithDateTime = DatePromptHelper.PrependDateTimePrompt(request.UserMessage, 7);

            var finalInput = BuildFinalInputWithTruncation(userMessageWithDateTime, session);

            contentParts.Add(new GeminiContentDto(
                "user", [new GeminiContentPartDto(finalInput)]
            ));

            foreach (var part in contentParts)
            {
                logger.LogInformation($"==== Content parts: {part.Parts.Select(p => p.Text).FirstOrDefault()}");
            }

            var payload = BuildGeminiMessagePayload(contentParts);

            var responseText = await CallGeminiAPIAsync(payload);

            if (string.IsNullOrWhiteSpace(responseText))
                throw new Exception("Failed to get a response from Gemini.");

            var aiMessages = SplitGeminiResponse(request.SessionId, responseText);

            await SaveMessagesAsync(request.SessionId, userId, request.UserMessage, request.SentAt, aiMessages);

            //Tự động tóm tắt nếu vượt ngưỡng
            await summarizationService.MaybeSummarizeSessionAsync(userId, request.SessionId);

            logger.LogInformation("Successfully processed message for session {SessionId}", request.SessionId);

            return aiMessages.Select(m => new AIMessageResponseDto(
                    m.SessionId,
                    m.SenderIsEmo,
                    m.Content,
                    m.CreatedDate))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message for session {SessionId}", request.SessionId);
            throw;
        }
        finally
        {
            lock (pendingQueue)
            {
                if (pendingQueue.Count > 0)
                    pendingQueue.Dequeue();

                //Nếu đã hết tin chờ thì cleanup queue khỏi dictionary
                if (pendingQueue.Count == 0)
                    PendingMessagesBySession.TryRemove(request.SessionId, out _);
            }
            
            sessionLock.Release();
        }
    }

    private static string BuildFinalInputWithTruncation(string userMessageWithDateTime, AIChatSession session)
    {
        var persona = session.PersonaSnapshot.ToPromptText();
        
        var trimmedUserMessage = userMessageWithDateTime.Length > MaxUserInputLength
            ? userMessageWithDateTime[..MaxUserInputLength] + "..."
            : userMessageWithDateTime;

        var notice = userMessageWithDateTime.Length > MaxUserInputLength
            ? "Ghi chú: nội dung người dùng đã được rút gọn vì quá dài.\n"
            : "";

        var finalInput = persona + "User:\n\n" + notice + trimmedUserMessage;
        return finalInput;
    }


    //Cleanup inactive sessions
    private static void CleanupInactiveSessions(object? state)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-SessionTimeoutMinutes);
        var inactiveSessions = ActiveSessions
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in inactiveSessions)
        {
            if (ActiveSessions.TryRemove(sessionId, out _) &&
                SessionLocks.TryRemove(sessionId, out var semaphore))
            {
                semaphore.Dispose();
            }
        }
    }

    //Thêm method SaveMessagesAsync
    private async Task SaveMessagesAsync(Guid sessionId, Guid userId, string userMessage, DateTime userMessageSentAt,
        List<AIMessage> aiResponse)
    {
        var lastSeq = await GetLastMessageSequenceIndex(sessionId);

        var nextSeq = lastSeq + 1;

        dbContext.AIChatMessages.Add(
            new AIMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderUserId = userId,
                SenderIsEmo = false,
                Content = userMessage,
                CreatedDate = userMessageSentAt,
                IsRead = true,
                BlockNumber = nextSeq
            });

        foreach (var message in aiResponse)
        {
            message.BlockNumber = nextSeq;
        }

        dbContext.AIChatMessages.AddRange(aiResponse);

        await dbContext.SaveChangesAsync();
    }

    private static List<AIMessage> SplitGeminiResponse(Guid sessionId, string responseText)
    {
        var aiMessages = responseText
            .Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => new AIMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SenderUserId = null,
                SenderIsEmo = true,
                Content = part,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            })
            .ToList();

        return aiMessages;
    }

    public async Task<AIMessage> AddMessageAsync(Guid sessionId, Guid userId, string content, bool senderIsEmo)
    {
        await ValidateSessionOwnershipAsync(sessionId, userId);

        var message = new AIMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            SenderUserId = senderIsEmo ? null : userId,
            SenderIsEmo = senderIsEmo,
            Content = content,
            CreatedDate = DateTime.UtcNow,
            IsRead = false
        };

        dbContext.AIChatMessages.Add(message);
        await dbContext.SaveChangesAsync();
        return message;
    }

    public async Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
    {
        await ValidateSessionOwnershipAsync(sessionId, userId);

        var unreadMessages = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId && !m.IsRead && !m.SenderIsEmo)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task<List<GeminiContentDto>> LoadSessionHistoryMessages(AIMessageRequestDto request, AIChatSession session)
    {
        //Lấy các message mới kể từ lần tóm tắt trước
        var lastIndex = session.LastSummarizedIndex ?? 0;

        var messages = await dbContext.AIChatMessages
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.CreatedDate)
            .Skip(lastIndex)
            .ToListAsync();

        var contentParts = new List<GeminiContentDto>();

        //Nếu có bản tóm tắt, bắt đầu từ đó
        if (!string.IsNullOrWhiteSpace(session.Summarization))
        {
            contentParts.Add(new GeminiContentDto(
                "user", [new GeminiContentPartDto($"Tóm tắt trước đó của cuộc hội thoại:\n{session.Summarization}")]
            ));
        }

        if (messages.Count == 0)
        {
            var lastSequenceIndex = await GetLastMessageSequenceIndex(request.SessionId);

            var lastMessageBlock = await dbContext.AIChatMessages
                .Where(m => m.SessionId == request.SessionId && m.BlockNumber == lastSequenceIndex)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            foreach (var message in lastMessageBlock)
            {
                contentParts.Add(new GeminiContentDto(
                    message.SenderIsEmo ? "model" : "user", [new GeminiContentPartDto($"{message.Content}")]
                ));
            }
        }

        //Thêm các message chưa tóm tắt vào
        foreach (var m in messages)
        {
            contentParts.Add(new GeminiContentDto(
                m.SenderIsEmo ? "model" : "user",
                [new GeminiContentPartDto(m.Content)]
            ));
        }

        return contentParts;
    }

    public async Task<PaginatedResult<AIMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId,
        PaginationRequest paginationRequest)
    {
        await ValidateSessionOwnershipAsync(sessionId, userId);

        var pageIndex = paginationRequest.PageIndex;
        var pageSize = paginationRequest.PageSize;

        ValidatePaginationRequest(pageIndex, pageSize);

        var query = dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedDate);

        var totalCount = await query.LongCountAsync();

        var messages = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedDate)
            .ProjectToType<AIMessageDto>()
            .ToListAsync();

        return new PaginatedResult<AIMessageDto>(pageIndex, pageSize, totalCount, messages);
    }

    private static void ValidatePaginationRequest(int pageIndex, int pageSize)
    {
        if (pageIndex <= 0 || pageSize <= 0)
            throw new ArgumentException("Tham số phân trang không hợp lệ.");
    }


    private async Task ValidateSessionOwnershipAsync(Guid sessionId, Guid userId)
    {
        var session = await dbContext.AIChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true);

        if (session == null)
            throw new UnauthorizedAccessException(
                "Không tìm thấy phiên trò chuyện hoặc phiên trò chuyện này không thuộc về người dùng.");
    }

    private GeminiRequestDto BuildGeminiMessagePayload(List<GeminiContentDto> contents)
    {
        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(_config.SystemInstruction)),
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

    private async Task<string> GetAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    private async Task<int> GetLastMessageSequenceIndex(Guid sessionId)
    {
        var lastSeq = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.BlockNumber)
            .Select(m => m.BlockNumber)
            .FirstOrDefaultAsync();

        return lastSeq;
    }

    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var token = await GetAccessTokenAsync();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url =
            $"https://{_config.Location}-aiplatform.googleapis.com/v1/projects/{_config.ProjectId}/locations/{_config.Location}/endpoints/{_config.EndpointId}:streamGenerateContent";
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
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
}