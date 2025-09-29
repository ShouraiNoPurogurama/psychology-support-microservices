using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Conversation.Application.Data;

/// <summary>
/// Application interface for Conversation write operations
/// Following the same pattern as IPostDbContext
/// </summary>
public interface IConversationDbContext
{
    DbSet<Domain.Aggregates.Conversations.Conversation> Conversations { get; }
    DbSet<Domain.Aggregates.Conversations.Message> Messages { get; }
    DbSet<Domain.Aggregates.Conversations.Participant> Participants { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
