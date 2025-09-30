using Media.Domain.Models;
using Media.Domain.Repositories;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Media.Infrastructure.Repositories
{
    public class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly MediaDbContext _dbContext;

        public MediaAssetRepository(MediaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(IEnumerable<Guid> mediaIds)
        {
            return await _dbContext.MediaAssets
                .Include(x => x.Variants) 
                .Where(x => mediaIds.Contains(x.Id))
                .ToListAsync();
        }
    }
}
