using Conversation.Domain.Aggregates.Conversations;
using Microsoft.EntityFrameworkCore;

namespace Conversation.Infrastructure.Data;

using Conversation = Domain.Aggregates.Conversations.Conversation;

public class ConversationDbContext : DbContext
{
    public ConversationDbContext(DbContextOptions<ConversationDbContext> options) : base(options)
    {
    }
    
    DbSet<Conversation> Conversations => Set<Conversation>();
    DbSet<Message> Messages => Set<Message>();
    DbSet<Participant> Participants => Set<Participant>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}