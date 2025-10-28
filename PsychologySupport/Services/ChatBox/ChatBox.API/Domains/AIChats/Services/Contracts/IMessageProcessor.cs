using BuildingBlocks.Pagination;
using ChatBox.API.Domains.AIChats.Dtos.AI;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IMessageProcessor
{
    Task<List<AIMessageResponseDto>> ProcessMessageAsync(AIMessageRequestDto request, Guid userId);

    Task<PaginatedResult<AIMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId,
        PaginationRequest paginationRequest);

    Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
}