using BuildingBlocks.Enums;

namespace UserMemory.API.Shared.Services.Contracts;

public interface ICurrentUserSubscriptionAccessor
{
    SubscriptionTier GetCurrentTier();
}