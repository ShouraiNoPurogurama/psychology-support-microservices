// using Microsoft.EntityFrameworkCore;
// using Post.Domain.Legacy.Comments;
// using Post.Domain.Legacy.Posts;
// using Post.Domain.Legacy.Reactions;
//
// namespace Post.Application.Data;
//
// public interface ILegacyPublicDbContext
// {
//     DbSet<CategoryTag> CategoryTags { get; set; }
//
//     DbSet<Comment> Comments { get; set; }
//
//     // DbSet<EmotionTag> EmotionTags { get; set; }
//
//     DbSet<GiftsAttach> GiftsAttaches { get; set; }
//     
//     DbSet<Domain.Legacy.Posts.Post> Posts { get; set; }
//
//     DbSet<PostCategory> PostCategories { get; set; }
//
//     DbSet<PostCounterDelta> PostCounterDeltas { get; set; }
//
//     DbSet<PostEmotion> PostEmotions { get; set; }
//
//     DbSet<PostMedia> PostMedia { get; set; }
//
//     DbSet<Reaction> Reactions { get; set; }
//     
//     Task<int> SaveChangesAsync(CancellationToken cancellationToken);
// }