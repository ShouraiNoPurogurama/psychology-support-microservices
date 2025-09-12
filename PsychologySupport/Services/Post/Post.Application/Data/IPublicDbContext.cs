using Microsoft.EntityFrameworkCore;
using Post.Domain.Comments;
using Post.Domain.Posts;
using Post.Domain.Reactions;

namespace Post.Application.Data;

public interface IPublicDbContext
{
    DbSet<CategoryTag> CategoryTags { get; set; }

    DbSet<Comment> Comments { get; set; }

    // DbSet<EmotionTag> EmotionTags { get; set; }

    DbSet<GiftsAttach> GiftsAttaches { get; set; }
    
    DbSet<Domain.Posts.Post> Posts { get; set; }

    DbSet<PostCategory> PostCategories { get; set; }

    DbSet<PostCounterDelta> PostCounterDeltas { get; set; }

    DbSet<PostEmotion> PostEmotions { get; set; }

    DbSet<PostMedia> PostMedia { get; set; }

    DbSet<Reaction> Reactions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}