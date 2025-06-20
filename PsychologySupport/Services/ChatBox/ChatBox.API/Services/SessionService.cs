using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using ChatBox.API.Data;
using ChatBox.API.Dtos.Sessions;
using ChatBox.API.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Services;

public class SessionService(ChatBoxDbContext dbContext)
{
    public async Task<AIChatSession> CreateSessionAsync(string sessionName, Guid userId)
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
        await dbContext.SaveChangesAsync();
        return session;
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