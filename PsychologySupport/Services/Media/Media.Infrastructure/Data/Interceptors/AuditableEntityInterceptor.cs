using Media.Domain.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;


namespace Media.Infrastructure.Data.Interceptors
{
    public class AuditableEntityInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = new())
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext? context)
        {
            if (context is null) return;

            var now = DateTimeOffset.UtcNow;
            var currentUser = GetCurrentUser();

            foreach (EntityEntry<IEntity> entry in context.ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity is IHasCreationAudit c)
                {
                    c.CreatedAt = now;
                    c.CreatedBy ??= currentUser;
                }

                if ((entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                    && entry.Entity is IHasModificationAudit m)
                {
                    m.LastModified = now;
                    m.LastModifiedBy = currentUser;
                }
            }
        }

        private string GetCurrentUser()
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext?.User.Identity?.IsAuthenticated != true) return "System";

            var subjectRef = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var sub = httpContext.User.FindFirst("sub")?.Value;

            return subjectRef ?? sub ?? "Unknown";
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r => r.TargetEntry != null &&
                                      r.TargetEntry.Metadata.IsOwned() &&
                                      r.TargetEntry.State is EntityState.Added or EntityState.Modified);
    }
}
