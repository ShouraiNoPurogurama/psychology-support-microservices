using Conversation.Application.Data;
using Conversation.Domain.Aggregates.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Conversation.Infrastructure.Data;

using Conversation = Domain.Aggregates.Conversations.Conversation;

public class ConversationDbContext : DbContext, IConversationDbContext
{
    public ConversationDbContext(DbContextOptions<ConversationDbContext> options) : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; }
    
    public DbSet<Message> Messages { get; }
    
    public DbSet<Participant> Participants { get; }
    
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}