using Microsoft.EntityFrameworkCore;
using Post.Application.ReadModels;

namespace Post.Application.Data;

public interface IQueryDbContext
{
    DbSet<EmotionTagReplica> EmotionTagReplicas { get; set; }
    DbSet<UserOwnedTagReplica> UserOwnedTagReplicas { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}