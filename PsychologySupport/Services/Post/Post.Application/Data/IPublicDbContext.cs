using Microsoft.EntityFrameworkCore;
using Post.Domain.Models;
using Post.Domain.Models.Public;

namespace Post.Application.Data;

public interface IPublicDbContext
{
    DbSet<CategoryTag> CategoryTags { get; set; }

    DbSet<Comment> Comments { get; set; }

    // DbSet<EmotionTag> EmotionTags { get; set; }

    DbSet<GiftsAttach> GiftsAttaches { get; set; }

    DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    DbSet<OutboxMessage> OutboxMessages { get; set; }

    DbSet<Domain.Models.Public.Post> Posts { get; set; }

    DbSet<PostCategory> PostCategories { get; set; }

    DbSet<PostCounterDelta> PostCounterDeltas { get; set; }

    DbSet<PostEmotion> PostEmotions { get; set; }

    DbSet<PostMedia> PostMedia { get; set; }

    DbSet<Reaction> Reactions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}