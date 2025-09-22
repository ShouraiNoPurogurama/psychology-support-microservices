namespace Alias.API.Common.Reservations;

public interface IAliasReservationStore
{
    Task<bool> ReserveAsync(string aliasKey, string aliasLabel, TimeSpan ttl, CancellationToken ct);
    Task<(bool ok, string? label)> TryConsumeAsync(string aliasKey, CancellationToken ct);
    Task<bool> ExistsAsync(string aliasKey, CancellationToken ct);
    Task<bool> RemoveAsync(string aliasKey, CancellationToken ct);
}