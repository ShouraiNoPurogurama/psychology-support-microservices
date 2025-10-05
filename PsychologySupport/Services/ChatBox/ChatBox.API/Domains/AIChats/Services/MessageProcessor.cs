using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Abstractions;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Utils;
using ChatBox.API.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

public class MessageProcessor(
    IContextBuilder contextBuilder,
    IAIProvider aiProvider,
    ISessionConcurrencyManager concurrencyManager,
    ChatBoxDbContext dbContext,
    SummarizationService summarizationService,
    IInstructionGenerator instructionGenerator,
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
        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .FirstAsync(s => s.Id == request.SessionId);

        var userMessageWithDateTime = DatePromptHelper.PrependDateTimePrompt(request.UserMessage, 7);

        // BƯỚC 1: Xây dựng ngữ cảnh ban đầu
        var initialContext = await contextBuilder.BuildContextAsync(request.SessionId, userMessageWithDateTime);

        // BƯỚC 2 (MỚI): Gọi "Instructor" để lấy gợi ý
        var instruction = await instructionGenerator.GenerateInstructionAsync(
            userMessageWithDateTime,
            session.Summarization,
            session.PersonaSnapshot.ToPromptText()
        );


        // BƯỚC 3 (MỚI): Chèn gợi ý vào ngữ cảnh
        // Nếu có instruction, chèn nó vào giữa phần persona/context và tin nhắn của người dùng
        // Điều này giúp AI chính đọc được chỉ dẫn ngay trước khi "thấy" tin nhắn cần trả lời
        var augmentedContext = initialContext;
        string knowledgeAugmentation = "";

        // 3.1: Kiểm tra Marker RAG để tạo khối Kiến thức
        if (instruction.Contains("RAG_TEAM_KNOWLEDGE"))
        {
            var teamKnowledge = await LoadTeamKnowledgeFileAsync();

            // Tạo khối kiến thức để chèn vào prompt của AI Emo
            knowledgeAugmentation = $@"
[KIẾN THỨC BỔ SUNG VỀ DỰ ÁN EMOEASE (BẮT BUỘC SỬ DỤNG)]
            {teamKnowledge}
[HẾT KIẾN THỨC BỔ SUNG]
            ";
            instruction = instruction.Replace("[MARKER: RAG_TEAM_KNOWLEDGE]", "").Trim();
        }

        // 3.2: Chèn Instruction và Knowledge vào ngữ cảnh
        if (!string.IsNullOrWhiteSpace(instruction) || !string.IsNullOrWhiteSpace(knowledgeAugmentation))
        {
            // Tách phần tin nhắn người dùng ra khỏi context
            var userMessageMarker = "[User]\n";
            var markerIndex = initialContext.IndexOf(userMessageMarker, StringComparison.Ordinal);

            if (markerIndex != -1)
            {
                var contextWithoutUserMessage = initialContext.Substring(0, markerIndex);
                var userMessagePart = initialContext.Substring(markerIndex);

                // Ghép lại: [Context cũ] + [Knowledge (nếu có)] + [Instruction (nếu có)] + [User Message]
                augmentedContext =
                    $"{contextWithoutUserMessage}" +
                    $"{knowledgeAugmentation}" + // Chèn kiến thức
                    $"\n\n{instruction}\n\n" + // Chèn gợi ý
                    $"{userMessagePart}";
            }
            else
            {
                // Trường hợp không tìm thấy marker tin nhắn người dùng, vẫn cố gắng chèn
                augmentedContext =
                    $"{initialContext}" +
                    $"{knowledgeAugmentation}" +
                    $"\n\n[INSTRUCTION TỪ COACH]\n{instruction}";
            }
        }

        logger.LogInformation("Augmented context with instruction: {Context}", augmentedContext);

        // BƯỚC 4: Xây dựng payload với ngữ cảnh đã tăng cường
        var payload = await BuildAIPayload(request, session, augmentedContext);

        var responseText = await aiProvider.GenerateResponseAsync(payload, session.Id);

        if (string.IsNullOrWhiteSpace(responseText))
            throw new Exception("Lấy response từ AI thất bại.");

        var aiMessages = SplitAIResponse(request.SessionId, responseText);
        await SaveMessagesAsync(request.SessionId, userId, request.UserMessage, request.SentAt, aiMessages);
        await summarizationService.MaybeSummarizeSessionAsync(userId, request.SessionId);

        return aiMessages.Select(m => new AIMessageResponseDto(
                m.SessionId, m.SenderIsEmo, m.Content, m.CreatedDate))
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

    private async Task<AIRequestPayload> BuildAIPayload(AIMessageRequestDto request, AIChatSession session,
        string augmentedContext)
    {
        var summarization = await GetSessionSummarizationAsync(session.Id);
        var historyMessages = await GetHistoryMessagesAsync(session.Id);

        return new AIRequestPayload(
            Context: augmentedContext,
            Summarization: summarization,
            HistoryMessages: historyMessages
        );
    }

    private async Task<string?> GetSessionSummarizationAsync(Guid sessionId)
    {
        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => s.Summarization)
            .FirstOrDefaultAsync();

        return session;
    }

    private async Task<List<HistoryMessage>> GetHistoryMessagesAsync(Guid sessionId)
    {
        const int maxHistoryMessages = 10;

        var session = await dbContext.AIChatSessions
            .AsNoTracking()
            .FirstAsync(s => s.Id == sessionId);

        var lastSummarizedIndex = session.LastSummarizedIndex ?? 0;

        var messages = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedDate)
            .Skip(lastSummarizedIndex + 1)
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