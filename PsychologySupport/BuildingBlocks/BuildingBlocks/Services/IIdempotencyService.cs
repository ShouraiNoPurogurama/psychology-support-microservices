namespace BuildingBlocks.Services;

public interface IIdempotencyService
{
        Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken cancellationToken = default);

        Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken cancellationToken = default);

        Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken cancellationToken = default);
   
}
