﻿using System.Security.Claims;
using BuildingBlocks.DDD;
using BuildingBlocks.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Data.Interceptors;

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

        var currentUser = GetCurrentUser();

        foreach (EntityEntry<IEntity> entry in context.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUser;
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow.AddHours(7);
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.LastModifiedBy = currentUser;
                entry.Entity.LastModified = DateTimeOffset.UtcNow.AddHours(7);
            }
        }
    }

    private string GetCurrentUser()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User.Identity?.IsAuthenticated != true) return "System";

        var userName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        
        var userId = httpContext.User.FindFirst("userId")?.Value
                     ?? httpContext.User.FindFirst("sub")?.Value;
        
        return userName ?? userId ?? "Unknown";
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r => r.TargetEntry != null &&
                                  r.TargetEntry.Metadata.IsOwned() &&
                                  r.TargetEntry.State is EntityState.Added or EntityState.Modified);
}