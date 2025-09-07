using Microsoft.EntityFrameworkCore;
using Post.Domain.Models.Query;

namespace Post.Application.Data;

public interface IQueryDbContext
{
    DbSet<EmotionTagReplica> EmotionTagReplicas { get; set; }
    DbSet<UserOwnedTagReplica> UserOwnedTagReplicas { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}