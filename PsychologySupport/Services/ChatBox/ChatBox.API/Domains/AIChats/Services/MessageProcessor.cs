using System.Text.Json;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Sessions;
using ChatBox.API.Domains.AIChats.Enums;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Domains.AIChats.Utils;
using ChatBox.API.Models;
using ChatBox.API.Shared.Authentication;
using Grpc.Core;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UserMemory.API.Protos;

namespace ChatBox.API.Domains.AIChats.Services;

public class MessageProcessor(
    IContextBuilder contextBuilder,
    IAIProvider aiProvider,
    IRouterClient routerClient,
    IPublishEndpoint publishEndpoint,
    ISessionConcurrencyManager concurrencyManager,
    ICurrentActorAccessor currentActorAccessor,
    UserMemorySearchService.UserMemorySearchServiceClient userMemorySearchClient,
    ChatBoxDbContext dbContext,
    SummarizationService summarizationService,
    ILogger<MessageProcessor> logger
)
    : IMessageProcessor
{
    public async Task<List<AIMessageResponseDto>> ProcessMessageAsync(AIMessageRequestDto request, Guid userId)
    {
        await ValidateSessionOwnershipAsync(request.SessionId, userId);

        //Check throttling
        if (concurrencyManager.ShouldThrottleMessage(request.SessionId, request.UserMessage))
        {
            return CreateThrottleResponse(request.SessionId);
        }

        //Track pending message
        concurrencyManager.TrackPendingMessage(request.SessionId, request.UserMessage);

        //Try acquire lock
        var lockAcquired = await concurrencyManager.TryAcquireSessionLockAsync(
            request.SessionId, TimeSpan.FromSeconds(15));

        if (!lockAcquired)
        {
            concurrencyManager.CompletePendingMessage(request.SessionId, request.UserMessage);
            throw new TimeoutException("Hệ thống đang xử lý tin nhắn khác. Vui lòng thử lại.");
        }

        try
        {
            return await ProcessMessageInternal(request, userId);
        }
        finally
        {
            concurrencyManager.CompletePendingMessage(request.SessionId, request.UserMessage);
            concurrencyManager.ReleaseSessionLock(request.SessionId);
        }
    }

    private List<AIMessageResponseDto> CreateThrottleResponse(Guid sessionId)
    {
        var throttleMessage = new AIMessageResponseDto(
            SessionId: sessionId,
            SenderIsEmo: true,
            Content: "Bạn đang gửi tin nhắn quá nhanh. Vui lòng chờ một chút trước khi gửi tin nhắn tiếp theo.",
            CreatedDate: DateTimeOffset.UtcNow
        );

        return new List<AIMessageResponseDto> { throttleMessage };
    }

    private async Task<List<AIMessageResponseDto>> ProcessMessageInternal(AIMessageRequestDto request, Guid userId)
    {
        var aliasId = currentActorAccessor.GetRequiredAliasId();

        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .FirstAsync(s => s.Id == request.SessionId);

        var userMessageWithDateTime = DatePromptHelper.PrependDateTimePrompt(request.UserMessage, 7);
        
        var historyMessages = await GetHistoryMessagesAsync(request.SessionId);

        // 1) Ngữ cảnh ban đầu
        var initialContext =  contextBuilder.BuildContextAsync(request.SessionId, userMessageWithDateTime);

        // 2) ROUTER
        var routerDecision = await routerClient.RouteAsync(
            userMessageWithDateTime,
            historyMessages,
            ct: CancellationToken.None
        );

        // Extract tags for progress tracking
        var messageTags = new List<string>();
        var saveNeeded = routerDecision?.SaveNeeded == true;

        // 2.1) SaveMemory nếu cần
        if (saveNeeded && routerDecision.MemoryToSave is not null)
        {
            var mem = routerDecision.MemoryToSave;
            var effectiveTags = new List<string>();
            if (mem.EmotionTags is { Count: > 0 }) effectiveTags.AddRange(mem.EmotionTags.Select(x => x.ToString()));
            if (mem.RelationshipTags is { Count: > 0 }) effectiveTags.AddRange(mem.RelationshipTags.Select(x => x.ToString()));
            if (mem.TopicTags is { Count: > 0 }) effectiveTags.AddRange(mem.TopicTags.Select(x => x.ToString()));

            messageTags = effectiveTags.Distinct().ToList();

            await publishEndpoint.Publish(new UserMemoryCreatedIntegrationEvent(
                AliasId: aliasId,
                SessionId: session.Id,
                Summary: mem.Summary,
                Tags: messageTags,
                SaveNeeded: true
            ));
        }

        // 2.2) ALWAYS publish MessageProcessed event for progress tracking (NEW)
        await publishEndpoint.Publish(new MessageProcessedIntegrationEvent(
            AliasId: aliasId,
            UserId: userId,
            SessionId: session.Id,
            UserMessage: request.UserMessage,
            SaveNeeded: saveNeeded,
            Tags: messageTags,
            ProcessedAt: DateTimeOffset.UtcNow
        ));

        logger.LogInformation(
            "Published MessageProcessedIntegrationEvent for AliasId={AliasId}, SessionId={SessionId}, SaveNeeded={SaveNeeded}",
            aliasId, session.Id, saveNeeded);

        // 2.3) gRPC RAG personal memory (MVP)
        string memoryAugmentation = string.Empty;
        string usageRules = string.Empty;
        var shouldCallMemory = routerDecision?.Intent == RouterIntent.RAG_PERSONAL_MEMORY
                               || routerDecision?.RetrievalNeeded == true;

        var instruction = routerDecision?.Instruction?.Trim();

        if (shouldCallMemory)
        {
            instruction += """

                           - BẮT BUỘC ground theo [NGỮ CẢNH KÝ ỨC CÁ NHÂN (DÙNG KHI PHÙ HỢP)] (nằm ở dưới); không mâu thuẫn hoặc bỏ qua.
                           - Không lặp nguyên văn; chỉ tham chiếu tinh tế.
                           """;

            var searchQuery = request.UserMessage.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var grpcReq = new SearchMemoriesRequest
                    {
                        AliasId = aliasId.ToString(),
                        Query = searchQuery,
                        TopK = 5,
                        MinScore = 0.5
                    };

                    var grpcResp = await userMemorySearchClient.SearchMemoriesAsync(grpcReq, cancellationToken: cts.Token);
                    if (grpcResp?.Hits?.Count > 0)
                    {
                        var tz = TimeSpan.FromHours(7);
                        var nowLocal = DateTimeOffset.UtcNow.ToOffset(tz);
                        
                        var bullets = grpcResp.Hits
                            .OrderByDescending(h => h.Score)
                            .Select(h =>
                            {
                                var ts = h.CapturedAt;
                                if (ts is null)
                                    return $"- [Điểm liên quan: {h.Score:0.00}] Ghi nhận: (không rõ ngày) {h.Summary}";

                                var whenLocal = ts.ToDateTimeOffset().ToOffset(tz);
                                var rel = TimeUtils.Relative(whenLocal, nowLocal);        // “2 tuần trước”
                                var dateStr = whenLocal.ToString("yyyy-MM-dd");         // 2025-10-30
                                return $"- [Điểm liên quan: {h.Score:0.00}] ({rel}) Ghi nhận {dateStr}: {h.Summary}";
                            });
                        
                        
                        memoryAugmentation =
                            $@"[NGỮ CẢNH KÝ ỨC CÁ NHÂN (DÙNG KHI PHÙ HỢP)]
{string.Join("\n", bullets)}
- Hôm nay là: {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)):yyyy-MM-dd}.
[/NGỮ CẢNH KÝ ỨC CÁ NHÂN]";

                        // 3.y) RULES dùng memory – tổng quát hoá cho mọi chủ đề
                        usageRules =
                            @"[QUY TẮC SỬ DỤNG KÝ ỨC CÁ NHÂN]
- Dùng ký ức để **định hướng tinh tế**, không áp đặt.
- **Không** lặp lại nguyên văn hoặc nêu đúng item đã lưu (tránh cảm giác bị theo dõi).
- Nếu có ràng buộc/giới hạn (dị ứng, thời gian, ngân sách, phong cách, sở thích), **ưu tiên lọc theo biên** khi gợi ý.
[/QUY TẮC SỬ DỤNG KÝ ỨC CÁ NHÂN]";
                    }
                }
                catch (RpcException ex)
                {
                    logger.LogWarning(ex, "UserMemory gRPC failed (RpcException) — continue without memory");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "UserMemory gRPC failed — continue without memory");
                }
            }
        }

        // 3) Team knowledge (nếu có)
        var augmentedContext = initialContext;
        if (routerDecision?.Intent == RouterIntent.RAG_TEAM_KNOWLEDGE)
        {
            var teamKnowledge = await LoadTeamKnowledgeFileAsync();
            var userMarker0 = "[User đang nhắn]:\n";
            var idx0 = initialContext.IndexOf(userMarker0, StringComparison.Ordinal);

            var block =
                $@"[KIẾN THỨC BỔ SUNG VỀ DỰ ÁN EMOEASE (BẮT BUỘC SỬ DỤNG)]
{teamKnowledge}
[HẾT KIẾN THỨC BỔ SUNG]";

            augmentedContext = idx0 != -1
                ? $"{initialContext[..idx0]}{block}\n\n{initialContext[idx0..]}"
                : $"{initialContext}\n\n{block}";
        }

        // 3.x) ROUTER INSTRUCTION – topic-agnostic, nói khéo (không “đọc thuộc” memory)
        string routerBlock = string.Empty;
        {
            // fallback nếu router không trả instruction hoặc trả sai format
            var safeInstruction =
                "[Gợi ý trả lời: Thừa nhận nhu cầu hiện tại; đề xuất 2–3 hướng lựa chọn tương phản (ví dụ: nhanh/gọn – thoải mái – tập trung/kỹ thuật); hỏi mở sở thích/giới hạn lúc này; tinh tế tham chiếu tới ‘gu gần đây’ nếu phù hợp, tránh nhắc đúng chi tiết đã lưu. Giọng điệu ấm áp, không áp đặt.]";

            if (string.IsNullOrWhiteSpace(instruction))
                instruction = safeInstruction;

            routerBlock =
                $@"[HƯỚNG DẪN TRẢ LỜI]
{instruction}
[/HƯỚNG DẪN TỪ TRẢ LỜI]";
        }

        // 3.z) Reorder: [System/Previous] -> [ROUTER INSTRUCTION] -> [USAGE RULES] -> [MEMORY] -> [User]
        {
            var userMarker = "[User đang nhắn]:\n";
            var idx = augmentedContext.IndexOf(userMarker, StringComparison.Ordinal);

            var head = idx != -1 ? augmentedContext[..idx] : augmentedContext;
            var tail = idx != -1 ? augmentedContext[idx..] : string.Empty;

            var middle = string.Join("\n\n", new[]
            {
                routerBlock,
                string.IsNullOrEmpty(usageRules) ? null : usageRules,
                string.IsNullOrEmpty(memoryAugmentation) ? null : memoryAugmentation
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            augmentedContext = string.IsNullOrWhiteSpace(middle)
                ? $"{head}\n{tail}"
                : $"{head}\n{middle}\n\n{tail}";
        }

        logger.LogInformation("Augmented context with instruction: {Context}", augmentedContext);

        // 4) Build payload
        var payload = await BuildAIPayload(historyMessages, session, augmentedContext);

        // 5) Call model
        var responseText = await aiProvider.GenerateResponseAsync_FoundationalModel(payload, session.Id);
        if (string.IsNullOrWhiteSpace(responseText))
            throw new Exception("Lấy response từ AI thất bại.");

        var aiMessages = SplitAIResponse(request.SessionId, responseText);

        // Persist
        await SaveMessagesAsync(request.SessionId, userId, request.UserMessage, request.SentAt, aiMessages);

        // Summarize if needed
        await summarizationService.MaybeSummarizeSessionAsync(userId, request.SessionId);

        // FE DTO
        return aiMessages
            .Select(m => new AIMessageResponseDto(m.SessionId, m.SenderIsEmo, m.Content, m.CreatedDate))
            .ToList();
    }


    private readonly string _knowledgeFilePath = Path.Combine(AppContext.BaseDirectory, "Lookups", "emoease_team_knowledge.md");

    private async Task<string> LoadTeamKnowledgeFileAsync()
    {
        if (File.Exists(_knowledgeFilePath))
        {
            return await File.ReadAllTextAsync(_knowledgeFilePath);
        }

        logger.LogError("File kiến thức đội nhóm không tìm thấy tại: {Path}", _knowledgeFilePath);
        return "Không thể truy xuất thông tin dự án vào lúc này.";
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

    //Helpers
    private async Task SaveMessagesAsync(Guid sessionId, Guid userId, string userMessage, DateTimeOffset userMessageSentAt,
        List<AIMessage> aiResponse)
    {
        var lastSeq = await GetLastMessageBlockIndex(sessionId);

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

    private async Task<AIRequestPayload> BuildAIPayload(List<HistoryMessage> historyMessages, AIChatSession session,
        string augmentedContext)
    {
        var summarization = await GetSessionSummarizationAsync(session.Id);

        logger.LogInformation("Summarization: {Summarization}", summarization);
        logger.LogInformation("HistoryMessages count: {Count}", historyMessages);

        return new AIRequestPayload(
            Context: augmentedContext,
            Summarization: summarization,
            HistoryMessages: historyMessages
        );
    }

    private async Task<string?> GetSessionSummarizationAsync(Guid sessionId)
    {
        var rawSummarization = await dbContext.AIChatSessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => s.Summarization)
            .FirstOrDefaultAsync();

        var summaryBlock = BuildChatSummaryBlock(rawSummarization);

        return summaryBlock;
    }

    private static string BuildChatSummaryBlock(string? rawSummarization)
    {
        if (string.IsNullOrWhiteSpace(rawSummarization))
            return "";

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var timeTag = $"[Thời gian hiện tại: {now:yyyy-MM-dd HH:mm}]";

        var parts = rawSummarization
                .Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries)
            ;

        var parsedParts = parts.Select(p =>
            {
                try
                {
                    var dto = JsonConvert.DeserializeObject<SummaryDto>(p.Trim().Replace("\n", ""));
                    if (dto == null) return null;
                    return new
                    {
                        current = dto.Current?.Trim(),
                        persist = dto.Persist?.Trim(),
                        capturedAt = dto.CreatedAt?.ToString()
                    };
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x is not null)
            .ToList();

        if (parsedParts.Count == 0) return "";

        // Chỉ lấy 2–3 đoạn gần nhất cho gọn
        var recent = parsedParts.TakeLast(3).ToList();

        var lines = new List<string>();
        foreach (var s in recent)
        {
            if (!string.IsNullOrWhiteSpace(s!.current))
                lines.Add($"- Context: {s.current}");
            if (!string.IsNullOrWhiteSpace(s.persist))
                lines.Add($"- Context bền vững: {s.persist}");
            if (!string.IsNullOrWhiteSpace(s.capturedAt))
                lines.Add($"- Lưu vào lúc: {s.capturedAt:dd/MM/yyyy}");
        }

        return $"{timeTag}.\n" + "Tóm tắt trước đó:\n" + string.Join("\n", lines);
    }

    private async Task<List<HistoryMessage>> GetHistoryMessagesAsync(Guid sessionId)
    {
        const int maxHistoryMessages = 10;
        const int reservedMessages = 4;
        
        var session = await dbContext.AIChatSessions.AsNoTracking().FirstAsync(s => s.Id == sessionId);
        
        var lastSummarizedIndex = session.LastSummarizedIndex ?? 0;
        
        var skipIndex = lastSummarizedIndex + 1 - reservedMessages >= 0 ? lastSummarizedIndex + 1 - reservedMessages : 0 ; 
        
        var messages = await dbContext.AIChatMessages.Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedDate)
            .Skip(skipIndex)
            .Take(maxHistoryMessages)
            .Select(m => new HistoryMessage(m.Content, m.SenderIsEmo))
            .ToListAsync();
        return messages;
    }


    private static List<AIMessage> SplitAIResponse(Guid sessionId, string responseText)
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
                CreatedDate = DateTimeOffset.UtcNow,
                IsRead = false
            })
            .ToList();

        return aiMessages;
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
            throw new ForbiddenException(
                "Không tìm thấy phiên trò chuyện hoặc phiên trò chuyện này không thuộc về người dùng.");
    }


    private async Task<int> GetLastMessageBlockIndex(Guid sessionId)
    {
        var lastBlock = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.BlockNumber)
            .Select(m => m.BlockNumber)
            .FirstOrDefaultAsync();

        return lastBlock;
    }
}