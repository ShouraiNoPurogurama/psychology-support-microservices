namespace UserMemory.API.Shared.Services.Contracts;

public interface ICurrentUserSubscriptionAccessor
{
    public bool IsFreeTier();
}