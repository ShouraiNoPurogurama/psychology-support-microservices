using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using ChatBox.API.Data;
using ChatBox.API.Dtos;
using ChatBox.API.Dtos.Sessions;
using ChatBox.API.Models;
using ChatBox.API.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Services;

public class SessionService(ChatBoxDbContext dbContext)
{
    public async Task<CreateSessionResponseDto> CreateSessionAsync(string sessionName, Guid userId)
    {
        var session = new AIChatSession
        {
            Id = Guid.NewGuid(),
            Name = sessionName,
            UserId = userId,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
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
        var greeting = EmoGreetingsUtil.GetRandomGreeting();

        var initialMessage = new AIMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            SenderUserId = null, //từ Emo
            SenderIsEmo = true,
            Content = greeting,
            CreatedDate = DateTime.UtcNow,
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
            ?? throw new NotFoundException($"Session {sessionId} not found or does not belong to the user.");
        
        return session;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId, Guid userId)
    {
        var session = await dbContext.AIChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true)
            
            ?? throw new NotFoundException($"Session {sessionId} not found or does not belong to the user.");

        session.IsActive = false;
        await dbContext.SaveChangesAsync();
        return true;
    }
    
    private static void ValidatePaginationRequest(int pageSize, int pageIndex)
    {
        if (pageSize <= 0 || pageIndex <= 0)
        {
            throw new ArgumentException("Invalid pagination parameters.");
        }
    }
}