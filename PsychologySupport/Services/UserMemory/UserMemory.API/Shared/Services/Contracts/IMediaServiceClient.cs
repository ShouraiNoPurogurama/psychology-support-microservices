using UserMemory.API.Shared.Dtos;

namespace UserMemory.API.Shared.Services.Contracts;

public interface IMediaServiceClient
{
    Task<MediaGenerationResultDto> GenerateStickerAsync(string prompt, Guid rewardId, CancellationToken ct);
}