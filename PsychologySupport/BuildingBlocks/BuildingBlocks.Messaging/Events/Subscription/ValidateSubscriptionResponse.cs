namespace BuildingBlocks.Messaging.Events.Subscription;

public record ValidateSubscriptionResponse(bool IsSuccess, List<string> Errors)
{
    public static ValidateSubscriptionResponse Success() => new(true, new List<string>());
    public static ValidateSubscriptionResponse Failed(string error) => new(false, new List<string> { error });
    public static ValidateSubscriptionResponse Failed(List<string> errors) => new(false, errors);
}