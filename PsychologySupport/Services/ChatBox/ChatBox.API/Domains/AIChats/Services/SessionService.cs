using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using ChatBox.API.Data;
using ChatBox.API.Data.Options;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Sessions;
using ChatBox.API.Domains.AIChats.Utils;
using ChatBox.API.Models;
using ChatBox.API.Shared.Authentication;
using ChatBox.API.Shared.Subscription;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Profile.API.Protos;

namespace ChatBox.API.Domains.AIChats.Services;

public class SessionService(
    ChatBoxDbContext dbContext,
    ICurrentActorAccessor actorAccessor,
    ICurrentUserSubscriptionAccessor subscriptionAccessor,
    PersonaOrchestratorService.PersonaOrchestratorServiceClient client)
{
    public async Task<CreateSessionResponseDto> CreateSessionAsync(string sessionName, Guid userId)
    {
        // 1. Resolve SubjectRef từ UserId
        var subjectRef = actorAccessor.GetRequiredSubjectRef();

        if (await dbContext.AIChatSessions.AnyAsync(s => s.UserId == userId
                                                         && s.IsActive == true
                                                         && s.IsLegacy))
            throw new ForbiddenException("Bạn đã có phiên trò chuyện chính. Không thể tạo phiên trò chuyện mới.");

        if (subscriptionAccessor.IsFreeTier())
        {
            var vnTz = TimeUtils.Instance;
            var nowVn = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vnTz);

            // [00:00, 24:00) theo giờ VN
            var startOfDayVn = new DateTimeOffset(nowVn.Year, nowVn.Month, nowVn.Day, 0, 0, 0, nowVn.Offset);
            var endOfDayVn = startOfDayVn.AddDays(1);

            var startUtc = startOfDayVn.ToUniversalTime();
            var endUtc = endOfDayVn.ToUniversalTime();

            var createdTodayCount = await dbContext.AIChatSessions
                .Where(s => s.UserId == userId
                            && s.IsActive == true
                            && s.CreatedDate >= startUtc
                            && s.CreatedDate < endUtc)
                .CountAsync();

            if (createdTodayCount >= QuotaOptions.SessionCreationFreeTier)
            {
                throw new ForbiddenException(
                    $"Gói miễn phí đã đạt giới hạn tạo phiên trong ngày {nowVn:yyyy-MM-dd} (GMT+7). " +
                    "Nâng cấp gói hoặc quay lại vào ngày mai nhé."
                );
            }
        }

        // 3. Gọi AggregatePatientProfileRequest với PatientId
        var profile = await client.GetPersonaSnapshotAsync(new GetPersonaSnapshotRequest()
        {
            SubjectRef = subjectRef.ToString()
        });

        // 4. Tạo PersonaSnapshot
        var persona = new PersonaSnapshot
        {
            Gender = profile.Gender.ToString(),
            BirthDate = profile.BirthDate.ToString(),
            JobTitle = profile.JobTitle,
        };

        // 5. Xử lý tên session
        sessionName = sessionName.Trim();
        var sessions = await dbContext.AIChatSessions
            .Where(s => s.UserId == userId && s.IsActive == true &&
                        (s.Name == sessionName || s.Name.StartsWith(sessionName + " ")))
            .ToListAsync();

        var maxSuffix = 1;
        var finalSessionName = sessionName;

        foreach (var parts in sessions.Select(s => s.Name.Split(' ')))
        {
            if (parts.Length > 1 && int.TryParse(parts.Last(), out var suffix))
            {
                if (suffix > maxSuffix)
                    maxSuffix = suffix;
            }
        }

        if (sessions.Any(s => s.Name == sessionName))
        {
            finalSessionName = $"{sessionName} {maxSuffix + 1}";
        }

        var session = new AIChatSession
        {
            Id = Guid.NewGuid(),
            Name = finalSessionName,
            UserId = userId,
            CreatedDate = DateTimeOffset.UtcNow,
            IsActive = true,
            PersonaSnapshot = persona
        };

        dbContext.AIChatSessions.Add(session);
        var initialGreeting = AddInitialGreeting(session);

        await dbContext.SaveChangesAsync();

        return new CreateSessionResponseDto(
            session.Id,
            session.Name,
            initialGreeting
        );
    }

    private AIMessageResponseDto AddInitialGreeting(AIChatSession session)
    {
        var greeting = EmoGreetingsUtil.GetOnboardingMessage(null);

        var initialMessage = new AIMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            SenderUserId = null, //từ Emo
            SenderIsEmo = true,
            Content = greeting,
            CreatedDate = DateTimeOffset.UtcNow,
            IsRead = false
        };

        dbContext.AIChatMessages.Add(initialMessage);

        return initialMessage.Adapt<AIMessageResponseDto>();
    }


    public async Task<PaginatedResult<GetSessionDto>> GetSessionsAsync(Guid userId, PaginationRequest paginationRequest)
    {
        var pageSize = paginationRequest.PageSize;
        var pageIndex = paginationRequest.PageIndex;

        ValidatePaginationRequest(pageSize, pageIndex);

        var query = dbContext.AIChatSessions
            .Where(s => s.UserId == userId && s.IsActive == true)
            .OrderByDescending(s => s.CreatedDate);

        var totalCount = await query.LongCountAsync();

        var sessions = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<GetSessionDto>()
            .ToListAsync();

        return new PaginatedResult<GetSessionDto>(pageIndex, pageSize, totalCount, sessions);
    }

    public async Task<AIChatSession> GetSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await dbContext.AIChatSessions
                          .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true)
                      ?? throw new NotFoundException(
                          $"Không tìm thấy phiên trò chuyện {sessionId} hoặc phiên không thuộc về người dùng.");


        return session;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await dbContext.AIChatSessions
                          .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true)
                      ?? throw new NotFoundException(
                          $"Không tìm thấy phiên trò chuyện {sessionId} hoặc phiên không thuộc về người dùng.");


        session.IsActive = false;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSessionAsync(AIChatSession newSession)
    {
        var currentSession = await dbContext.AIChatSessions
                                 .FirstOrDefaultAsync(s => s.Id == newSession.Id)
                             ?? throw new NotFoundException(
                                 $"Không tìm thấy phiên trò chuyện {newSession.Id} hoặc phiên không thuộc về người dùng.");

        newSession.Adapt(currentSession);

        var result = await dbContext.SaveChangesAsync();

        return result > 0;
    }

    private static void ValidatePaginationRequest(int pageSize, int pageIndex)
    {
        if (pageSize <= 0 || pageIndex <= 0)
        {
            throw new ArgumentException("Tham số phân trang không hợp lệ.");
        }
    }
}