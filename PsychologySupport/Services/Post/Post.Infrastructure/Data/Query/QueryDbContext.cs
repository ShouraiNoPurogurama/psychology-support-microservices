using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.ReadModels;
using Post.Application.ReadModels.Models;

namespace Post.Infrastructure.Data.Query;

public class QueryDbContext : DbContext, IQueryDbContext
{
    public DbSet<EmotionTagReplica> EmotionTagReplicas { get; set; }

    public DbSet<UserOwnedTagReplica> UserOwnedTagReplicas { get; set; }
    public DbSet<AliasVersionReplica> AliasVersionReplica { get; set; }

    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder buidler)
    {
        buidler.HasDefaultSchema("query");

        buidler.Entity<EmotionTagReplica>(e => { e.HasIndex(et => et.Code, "unq_EmotionTagReplica_Code").IsUnique(); });

        buidler.Entity<UserOwnedTagReplica>(e =>
        {
            //Subject ref đứng đầu => index sẽ được sử dụng khi lọc theo SubjectRef
            e.HasKey(u => new { SubjectRef = u.AliasId, u.EmotionTagId });
        });

        buidler.Entity<AliasVersionReplica>(entity =>
        {
            entity.HasKey(e => e.AliasId);
        });

        base.OnModelCreating(buidler);
    }
}