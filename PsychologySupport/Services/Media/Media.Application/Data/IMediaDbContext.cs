using Media.Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace Media.Application.Data
{
    public interface IMediaDbContext
    {
        DbSet<IdempotencyKey> IdempotencyKeys { get;}

        DbSet<MediaAsset> MediaAssets { get;}

        DbSet<MediaModerationAudit> MediaModerationAudits { get;}

        DbSet<MediaOwner> MediaOwners { get;}

        DbSet<MediaProcessingJob> MediaProcessingJobs { get;}

        DbSet<MediaVariant> MediaVariants { get;}

        DbSet<OutboxMessage> OutboxMessages { get;}

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
