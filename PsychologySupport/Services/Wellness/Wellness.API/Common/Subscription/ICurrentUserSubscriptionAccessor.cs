namespace Wellness.API.Common.Subscription;

public interface ICurrentUserSubscriptionAccessor
{
    public bool IsFreeTier();
    public string GetCurrentSubscription();
}