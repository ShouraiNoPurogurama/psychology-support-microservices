namespace UserMemory.API.Shared.Services.Contracts;

public interface ITagSyncService
{
    Task SyncAsync(CancellationToken ct = default);

}