using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.AI.Router;
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
    IMessagPreprocessor messagePreprocessor,
    IAIProvider aiProvider,
    IAIRequestFactory aiRequestFactory,
    IInstructionComposer instructionComposer,
    IRouterClient routerClient,
    IToolSelectorClient toolSelectorClient,
    IPublishEndpoint publishEndpoint,
    ISessionConcurrencyManager concurrencyManager,
    ICurrentActorAccessor currentActorAccessor,
    IConfiguration configuration,
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

        if (concurrencyManager.ShouldThrottleMessage(request.SessionId, request.UserMessage))
            return
            [
                new AIMessageResponseDto(
                    SessionId: request.SessionId,
                    SenderIsEmo: true,
                    Content: "Bạn đang gửi tin nhắn quá nhanh. Vui lòng chờ một chút.",
                    CreatedDate: DateTimeOffset.UtcNow
                )
            ];

        concurrencyManager.TrackPendingMessage(request.SessionId, request.UserMessage);

        var lockAcquired = await concurrencyManager.TryAcquireSessionLockAsync(
            request.SessionId, TimeSpan.FromSeconds(15));

        if (!lockAcquired)
        {
            concurrencyManager.CompletePendingMessage(request.SessionId, request.UserMessage);
            throw new TimeoutException("Hệ thống đang xử lý tin nhắn khác. Vui lòng thử lại.");
        }

        try
        {
            var result = await ProcessMessageInternal(request, userId);
            return result;
        }
        finally
        {
            concurrencyManager.CompletePendingMessage(request.SessionId, request.UserMessage);
            concurrencyManager.ReleaseSessionLock(request.SessionId);
        }
    }

    private async Task<List<AIMessageResponseDto>> ProcessMessageInternal(AIMessageRequestDto request, Guid userId)
    {
        var aliasId = currentActorAccessor.GetRequiredAliasId();
        var session = await dbContext.AIChatSessions.AsNoTracking().FirstAsync(s => s.Id == request.SessionId);

        // 1) chuẩn hoá input & history/context
        var userMessageWithDateTime = DatePromptHelper.PrependDateTimePrompt(request.UserMessage, 7);
        
        var history = await GetHistoryMessagesAsync(request.SessionId);
        
        var initialContext = messagePreprocessor.FormatUserMessageBlock(userMessageWithDateTime);

        // 2) gọi router + extract snapshot
        var router = await RouteAndExtractIntentsAsync(userMessageWithDateTime, history);
        
        RouterToolType? selectedToolType = null;
        CtaBlock? ctaResult = null;
        
        if (router.Intent == RouterIntent.TOOL_CALLING)
        {
            logger.LogInformation("Router intent is TOOL_CALLING, invoking ToolSelector...");
            var toolDecision = await toolSelectorClient.SelectToolAsync(userMessageWithDateTime, history);
            
            if (toolDecision != null && toolDecision.ToolCall.Needed)
            {
                selectedToolType = toolDecision.ToolCall.Type;
                ctaResult = toolDecision.Cta; // Lấy CTA từ specialist
                logger.LogInformation("ToolSelector decided: {ToolType}", selectedToolType);
            }
        }

        // 3) save memory nếu cần + publish MessageProcessed
        var tags = await SaveMemoryIfNeededAsync(router, aliasId, session.Id);
        await PublishProcessedEventAsync(aliasId, userId, session.Id, request.UserMessage, router.SaveNeeded, tags);

        // 4) augmentation (personal memory / team knowledge)
        var (augmentedContext, instructionForAnswer) = await BuildAugmentedContextAsync(
            initialContext,
            request.UserMessage,
            router,
            aliasId
        );

        var finalSystemInstruction = instructionComposer.Compose(
            intent: router.Intent,
            toolType: selectedToolType,
            basePersona: configuration["GeminiConfig:SystemInstruction"]!,
            routerGuidance: instructionForAnswer,
            extraGuards: null
        );

        // 5) gọi model chính + persist + summarize
        var result = await GenerateAIResponseWithSummarizationAsync(
            sessionId: request.SessionId,
            userId: userId,
            userMessage: request.UserMessage,
            sentAt: request.SentAt,
            history: history,
            session: session,
            augmentedContext: augmentedContext,
            systemInstruction: finalSystemInstruction
        );
        
        if (ctaResult != null && result.Count > 0)
        {
            var firstMessage = result[0]; 
    
            var messageWithCta = firstMessage with { Cta = ctaResult };

            result[0] = messageWithCta; 
        }

        return result;
    }

    private async Task<RouterSnapshot> RouteAndExtractIntentsAsync(
        string userMessageWithDateTime,
        List<HistoryMessage> history)
    {
        var decision = await routerClient.RouteAsync(userMessageWithDateTime, history, CancellationToken.None);

        var intent = decision?.Route.Intent ?? RouterIntent.CONVERSATION;
        // var instruction = decision?.Guidance.EmoInstruction.Trim() ?? string.Empty;
        
        var instruction =  string.Empty;

        var retrievalNeeded = decision?.Retrieval.Needed == true;
        var scopes = decision?.Retrieval.Scopes;
        var usePersonal = scopes?.PersonalMemory == true;
        var useTeam = scopes?.TeamKnowledge == true;

        var saveNeeded = decision?.Memory.Save.Needed == true;
        var payload = decision?.Memory.Save.Payload;
        
        RouterToolType? toolType = null;

        return new RouterSnapshot(
            intent,
            retrievalNeeded,
            usePersonal,
            useTeam,
            saveNeeded,
            payload,
            instruction,
            toolType
        );
    }

    private async Task<List<string>> SaveMemoryIfNeededAsync(
        RouterSnapshot router,
        Guid aliasId,
        Guid sessionId)
    {
        var tags = new List<string>();

        if (!router.SaveNeeded || router.MemoryPayload is null)
            return tags;

        var mem = router.MemoryPayload;

        if (mem.EmotionTags is { Count: > 0 }) tags.AddRange(mem.EmotionTags.Select(x => x.ToString()));
        if (mem.RelationshipTags is { Count: > 0 }) tags.AddRange(mem.RelationshipTags.Select(x => x.ToString()));
        if (mem.TopicTags is { Count: > 0 }) tags.AddRange(mem.TopicTags.Select(x => x.ToString()));

        tags = tags.Distinct().ToList();

        await publishEndpoint.Publish(new UserMemoryCreatedIntegrationEvent(
            AliasId: aliasId,
            SessionId: sessionId,
            Summary: mem.Summary,
            Tags: tags,
            SaveNeeded: true
        ));

        return tags;
    }

    private Task PublishProcessedEventAsync(
        Guid aliasId,
        Guid userId,
        Guid sessionId,
        string userMessage,
        bool saveNeeded,
        List<string> tags)
    {
        return publishEndpoint.Publish(new MessageProcessedIntegrationEvent(
            AliasId: aliasId,
            UserId: userId,
            SessionId: sessionId,
            UserMessage: userMessage,
            SaveNeeded: saveNeeded,
            Tags: tags,
            ProcessedAt: DateTimeOffset.UtcNow
        ));
    }

    private async Task<(string augmentedContext, string instructionForAnswer)> BuildAugmentedContextAsync(
        string initialContext,
        string userMessage,
        RouterSnapshot router,
        Guid aliasId)
    {
        var instruction = router.Instruction;
        var usageRules = string.Empty;
        var memoryBlock = string.Empty;

        // 1) personal memory augmentation nếu cần
        var needPersonal =
            router.Intent == RouterIntent.RAG_PERSONAL_MEMORY
            || router.RetrievalNeeded
            || router.UsePersonalMemory;

        if (needPersonal)
        {
            instruction = EnsureInstructionWithPersonalMemoryHints(instruction);

            (memoryBlock, usageRules) =
                await GetPersonalMemoryAugmentationAsync(userMessage, aliasId);
        }

        // 2) team knowledge augmentation nếu cần
        var contextAfterTeam = initialContext;
        if (router.Intent == RouterIntent.RAG_TEAM_KNOWLEDGE || router.UseTeamKnowledge)
        {
            contextAfterTeam = await InjectTeamKnowledgeAsync(initialContext);
        }

        // 3) fallback instruction
        var finalInstruction = string.IsNullOrWhiteSpace(instruction)
            ? "[Gợi ý trả lời: Thừa nhận cảm xúc; đưa 2–3 hướng gợi ý; hỏi mở nhẹ 1 câu; giọng ấm áp.]"
            : instruction;

        // 4) reorder: [head] -> [INSTRUCTION] -> [USAGE RULES] -> [MEMORY] -> [tail]
        var augmented = ReorderContextWithRouterBlocks(contextAfterTeam, finalInstruction, usageRules, memoryBlock);

        logger.LogInformation("Augmented context: {Context}", augmented);

        return (augmented, finalInstruction);
    }

    private static string EnsureInstructionWithPersonalMemoryHints(string instruction)
    {
        var baseText = string.IsNullOrWhiteSpace(instruction) ? "" : instruction.Trim();
        return baseText + """

                          - BẮT BUỘC ground theo [NGỮ CẢNH KÝ ỨC CÁ NHÂN].
                          - Không lặp nguyên văn item đã lưu; chỉ tham chiếu tinh tế.
                          """;
    }

    private async Task<(string memoryBlock, string usageRules)> GetPersonalMemoryAugmentationAsync(
        string userMessage,
        Guid aliasId)
    {
        var memoryBlock = string.Empty;
        var usageRules = string.Empty;

        var searchQuery = userMessage.Trim();
        if (string.IsNullOrEmpty(searchQuery))
            return (memoryBlock, usageRules);

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
                            return $"- [Score: {h.Score:0.00}] (không rõ ngày) {h.Summary}";

                        var whenLocal = ts.ToDateTimeOffset().ToOffset(tz);
                        var rel = TimeUtils.Relative(whenLocal, nowLocal);
                        var dateStr = whenLocal.ToString("dd/MM/yyyy");
                        return $"- [Score: {h.Score:0.00}] ({rel}) {dateStr}: {h.Summary}";
                    });

                memoryBlock =
                    $@"[NGỮ CẢNH KÝ ỨC CÁ NHÂN]
{string.Join("\n", bullets)}
[/NGỮ CẢNH KÝ ỨC CÁ NHÂN]";

                usageRules =
                    @"[QUY TẮC DÙNG KÝ ỨC]
- Dùng ký ức để định hướng tinh tế.
- Không lặp nguyên văn item lưu.
- Nếu user có ràng buộc (dị ứng, sở thích...), ưu tiên gợi ý theo biên.
[/QUY TẮC DÙNG KÝ ỨC]";
            }
        }
        catch (RpcException ex)
        {
            logger.LogWarning(ex, "UserMemory gRPC failed (Rpc)");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "UserMemory gRPC failed");
        }

        return (memoryBlock, usageRules);
    }

    private async Task<string> InjectTeamKnowledgeAsync(string initialContext)
    {
        var teamKnowledge = await LoadTeamKnowledgeFileAsync();
        var userMarker = "[User đang nhắn]:\n";
        var idx = initialContext.IndexOf(userMarker, StringComparison.Ordinal);

        var block =
            $@"[KIẾN THỨC DỰ ÁN EMOEASE]
{teamKnowledge}
[/KIẾN THỨC DỰ ÁN EMOEASE]";

        return idx != -1
            ? $"{initialContext[..idx]}{block}\n\n{initialContext[idx..]}"
            : $"{initialContext}\n\n{block}";
    }

    private static string ReorderContextWithRouterBlocks(
        string context,
        string instruction,
        string usageRules,
        string memoryBlock)
    {
        var userMarker = "[User đang nhắn]:\n";
        var idx = context.IndexOf(userMarker, StringComparison.Ordinal);

        var head = idx != -1 ? context[..idx] : context;
        var tail = idx != -1 ? context[idx..] : string.Empty;

        var routerBlock =
            $@"[HƯỚNG DẪN TRẢ LỜI]
{instruction}
[/HƯỚNG DẪN TRẢ LỜI]";

        var middle = string.Join("\n\n", new[]
        {
            routerBlock,
            string.IsNullOrWhiteSpace(usageRules) ? null : usageRules,
            string.IsNullOrWhiteSpace(memoryBlock) ? null : memoryBlock
        }.Where(x => x != null));

        return string.IsNullOrWhiteSpace(middle)
            ? $"{head}\n{tail}"
            : $"{head}\n{middle}\n\n{tail}";
    }


    private async Task<List<AIMessageResponseDto>> GenerateAIResponseWithSummarizationAsync(
        Guid sessionId,
        Guid userId,
        string userMessage,
        DateTimeOffset sentAt,
        List<HistoryMessage> history,
        AIChatSession session,
        string augmentedContext,
        string systemInstruction)
    {
        var envelope = await aiRequestFactory.CreateAsync(history, session, augmentedContext);
        
        var responseText = await aiProvider.GenerateChatResponseAsync(
            envelope,
            systemInstruction,
            CancellationToken.None
        );

        if (string.IsNullOrWhiteSpace(responseText))
            throw new Exception("AI không trả về nội dung.");

        var aiMessages = SplitAIResponse(sessionId, responseText);

        await SaveMessagesAsync(sessionId, userId, userMessage, sentAt, aiMessages);
        await summarizationService.MaybeSummarizeSessionAsync(userId, sessionId);

        return aiMessages
            .Select(m => new AIMessageResponseDto(m.SessionId, m.SenderIsEmo, m.Content, m.CreatedDate))
            .ToList();
    }

    private readonly string _knowledgeFilePath = Path.Combine(AppContext.BaseDirectory, "Lookups", "emoease_team_knowledge.md");

    private async Task<string> LoadTeamKnowledgeFileAsync()
    {
        if (File.Exists(_knowledgeFilePath))
            return await File.ReadAllTextAsync(_knowledgeFilePath);

        logger.LogError("File team knowledge không tìm thấy: {Path}", _knowledgeFilePath);
        return "Không thể tải kiến thức dự án.";
    }

    public async Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId)
    {
        await ValidateSessionOwnershipAsync(sessionId, userId);

        var unread = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId && !m.IsRead && !m.SenderIsEmo)
            .ToListAsync();

        unread.ForEach(m => m.IsRead = true);
        await dbContext.SaveChangesAsync();
    }

    public async Task<PaginatedResult<AIMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId,
        PaginationRequest paginationRequest)
    {
        await ValidateSessionOwnershipAsync(sessionId, userId);

        var query = dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedDate);

        var totalCount = await query.LongCountAsync();
        var messages = await query
            .Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
            .Take(paginationRequest.PageSize)
            .OrderBy(m => m.CreatedDate)
            .ProjectToType<AIMessageDto>()
            .ToListAsync();

        return new PaginatedResult<AIMessageDto>(
            paginationRequest.PageIndex,
            paginationRequest.PageSize,
            totalCount,
            messages
        );
    }

    private async Task SaveMessagesAsync(Guid sessionId, Guid userId, string userMessage, DateTimeOffset sentAt,
        List<AIMessage> aiResponse)
    {
        var lastSeq = await GetLastMessageBlockIndex(sessionId);
        var nextSeq = lastSeq + 1;

        dbContext.AIChatMessages.Add(new AIMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            SenderUserId = userId,
            SenderIsEmo = false,
            Content = userMessage,
            CreatedDate = sentAt,
            IsRead = true,
            BlockNumber = nextSeq
        });

        foreach (var msg in aiResponse)
            msg.BlockNumber = nextSeq;

        dbContext.AIChatMessages.AddRange(aiResponse);
        await dbContext.SaveChangesAsync();
    }

    private async Task<string?> GetSessionSummarizationAsync(Guid sessionId)
    {
        var raw = await dbContext.AIChatSessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => s.Summarization)
            .FirstOrDefaultAsync();

        return BuildChatSummaryBlock(raw);
    }

    private static string BuildChatSummaryBlock(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var timeTag = $"[Thời gian hiện tại: {now:yyyy-MM-dd HH:mm}]";

        var parts = raw.Split(["---"], StringSplitOptions.RemoveEmptyEntries);

        var parsed = parts.Select(p =>
            {
                try
                {
                    var dto = JsonConvert.DeserializeObject<SummaryDto>(p.Trim().Replace("\n", ""));
                    return dto == null ? null : new { dto.Current, dto.Persist, dto.CreatedAt };
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x != null)
            .ToList();

        var recent = parsed.TakeLast(3).ToList();

        var lines = new List<string>();
        foreach (var s in recent)
        {
            if (!string.IsNullOrWhiteSpace(s!.Current))
                lines.Add($"- Ngữ cảnh tạm thời (điều đang diễn ra lúc đó): {s.Current}");

            if (!string.IsNullOrWhiteSpace(s.Persist))
                lines.Add($"- Ngữ cảnh dài hạn (thông tin mang tính bền vững): {s.Persist}");

            if (s.CreatedAt != null)
                lines.Add($"- Thời điểm được ghi nhận: {s.CreatedAt:yyyy-MM-dd}");
        }

        return $"{timeTag}\nTóm tắt trước đó:\n" + string.Join("\n", lines);
    }

    private async Task<List<HistoryMessage>> GetHistoryMessagesAsync(Guid sessionId)
    {
        const int maxHistory = 10;
        const int reserved = 4;

        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .FirstAsync(s => s.Id == sessionId);

        var lastSummaryIdx = session.LastSummarizedIndex ?? 0;

        var skip = lastSummaryIdx + 1 - reserved;
        if (skip < 0) skip = 0;

        return await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedDate)
            .Skip(skip)
            .Take(maxHistory)
            .Select(m => new HistoryMessage(m.Content, m.SenderIsEmo))
            .ToListAsync();
    }

    private static List<AIMessage> SplitAIResponse(Guid sessionId, string text)
    {
        return text
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
    }

    private async Task ValidateSessionOwnershipAsync(Guid sessionId, Guid userId)
    {
        var session = await dbContext.AIChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true);

        if (session == null)
            throw new ForbiddenException("Phiên trò chuyện không thuộc về bạn hoặc đã bị vô hiệu.");
    }

    private async Task<int> GetLastMessageBlockIndex(Guid sessionId)
    {
        return await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.BlockNumber)
            .Select(m => m.BlockNumber)
            .FirstOrDefaultAsync();
    }
}