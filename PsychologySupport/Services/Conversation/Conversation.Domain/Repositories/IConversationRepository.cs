
namespace Conversation.Domain.Repositories;

public interface IConversationRepository
{
    Task<Conversation.Domain.Aggregates.Conversations.Conversation?> GetByIdAsync(string conversationId, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Conversation.Domain.Aggregates.Conversations.Conversation>> GetByParticipantAsync(
        Guid participantAliasId, 
        CancellationToken cancellationToken = default);
    
    Task SaveAsync(Conversation.Domain.Aggregates.Conversations.Conversation conversation, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string conversationId, CancellationToken cancellationToken = default);
}
