using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Chatbox;
using BuildingBlocks.Pagination;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.Sessions;
using ChatBox.API.Domains.AIChats.Utils;
using ChatBox.API.Models;
using ChatBox.API.Shared.Authentication;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Profile.API.Protos;

namespace ChatBox.API.Domains.AIChats.Services;

public class SessionService(
    ChatBoxDbContext dbContext,
    ICurrentActorAccessor actorAccessor,
    ILogger<SessionService> logger,
    IPublishEndpoint publishEndpoint,
    PersonaOrchestratorService.PersonaOrchestratorServiceClient client)
{
    public async Task<CreateSessionResponseDto> CreateSessionAsync(string sessionName, Guid userId)
    {
        // 1. Resolve SubjectRef từ UserId
        var subjectRef = actorAccessor.GetRequiredSubjectRef();
        var aliasId = actorAccessor.GetRequiredAliasId();

        if (DevConfigs.IsDeveloperAccount(subjectRef) && await dbContext.AIChatSessions.AnyAsync(s => s.UserId == userId
                                                         && s.IsLegacy == false
                                                         && s.IsActive == true
            )
           )
            throw new ForbiddenException("Bạn đã có phiên trò chuyện chính. Không thể tạo phiên trò chuyện mới.", "SESSION_LIMIT_REACHED");

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
            PersonaSnapshot = persona,
            IsLegacy = false
        };

        dbContext.AIChatSessions.Add(session);
        var initialGreeting = AddInitialGreeting(session);

        try
        {
            await dbContext.SaveChangesAsync();
            var sessionCreatedIntegrationEvent = new SessionCreatedIntegrationEvent(SessionId: session.Id, AliasId: aliasId, UserId: userId);

            await publishEndpoint.Publish(sessionCreatedIntegrationEvent);
        }
        catch (Exception e)
        {
            logger.LogCritical("Failed initializing new session with exception stack trace: {ex}", e);
            throw;
        }

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