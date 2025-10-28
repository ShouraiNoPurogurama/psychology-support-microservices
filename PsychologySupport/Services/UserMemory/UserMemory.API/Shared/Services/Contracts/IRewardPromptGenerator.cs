using UserMemory.API.Shared.Dtos;

namespace UserMemory.API.Shared.Services.Contracts;

public interface IRewardPromptGenerator
{
    Task<RewardGenerationDataDto> PrepareGenerationDataAsync(Guid aliasId, Guid rewardId, Guid chatSessionId, CancellationToken ct);
}