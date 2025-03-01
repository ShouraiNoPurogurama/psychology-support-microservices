using System.Text.Json.Serialization;

namespace Subscription.API.Data.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled
}