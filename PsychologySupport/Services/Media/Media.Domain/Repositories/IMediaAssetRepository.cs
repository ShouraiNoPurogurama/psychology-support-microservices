using Media.Domain.Models;

namespace Media.Domain.Repositories
{
    public interface IMediaAssetRepository
    {
        Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IEnumerable<Guid> mediaIds);
    }
}
