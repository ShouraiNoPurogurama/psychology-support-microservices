using System.Text.Json.Serialization;

namespace Subscription.API.ServicePackages.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServicePackageBuyStatus
{
    Purchased,
    PendingPayment,
    NotPurchased
}