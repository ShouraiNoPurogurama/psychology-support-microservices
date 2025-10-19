using Microsoft.EntityFrameworkCore;
using Post.Application.ReadModels;
using Post.Application.ReadModels.Models;

namespace Post.Application.Data;

public interface IQueryDbContext
{
    DbSet<EmotionTagReplica> EmotionTagReplicas { get; set; }
    DbSet<UserOwnedTagReplica> UserOwnedTagReplicas { get; set; }
    DbSet<AliasVersionReplica> AliasVersionReplica { get; set; }
    DbSet<UserOwnedGiftReplica> UserOwnedGiftReplicas { get; set; }
    DbSet<GiftReplica> GiftReplicas { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}