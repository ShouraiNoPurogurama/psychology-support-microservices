using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.ReadModels;

namespace Post.Infrastructure.Data.Query;

public class QueryDbContext : DbContext, IQueryDbContext
{
    public DbSet<EmotionTagReplica> EmotionTagReplicas { get; set; }

    public DbSet<UserOwnedTagReplica> UserOwnedTagReplicas { get; set; }

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
            e.HasKey(u => new { u.SubjectRef, u.EmotionTagId });
        });

        base.OnModelCreating(buidler);
    }
}