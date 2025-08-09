using BuildingBlocks.Pagination;
using ChatBox.API.Dtos;
using ChatBox.API.Dtos.AI;

namespace ChatBox.API.Abstractions;

public interface IMessageProcessor
{
    Task<List<AIMessageResponseDto>> ProcessMessageAsync(AIMessageRequestDto request, Guid userId);

    Task<PaginatedResult<AIMessageDto>> GetMessagesAsync(Guid sessionId, Guid userId,
        PaginationRequest paginationRequest);

    Task MarkMessagesAsReadAsync(Guid sessionId, Guid userId);
}