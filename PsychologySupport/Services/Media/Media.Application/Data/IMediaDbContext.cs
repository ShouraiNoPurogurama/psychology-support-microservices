using Media.API.Media.Models;
using Microsoft.EntityFrameworkCore;


namespace Media.Application.Data
{
    public interface IMediaDbContext
    {
        DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

        DbSet<MediaAsset> MediaAssets { get; set; }

        DbSet<MediaModerationAudit> MediaModerationAudits { get; set; }

        DbSet<MediaOwner> MediaOwners { get; set; }

        DbSet<MediaProcessingJob> MediaProcessingJobs { get; set; }

        DbSet<MediaVariant> MediaVariants { get; set; }

        DbSet<OutboxMessage> OutboxMessages { get; set; }
    }
}
