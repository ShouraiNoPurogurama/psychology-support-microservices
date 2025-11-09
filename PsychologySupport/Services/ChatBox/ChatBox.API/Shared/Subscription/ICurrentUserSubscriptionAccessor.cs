namespace ChatBox.API.Shared.Subscription;

public interface ICurrentUserSubscriptionAccessor
{
    public bool IsFreeTier();
    public string GetCurrentSubscription();
}