using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Subscription.API.Data.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionStatus
{
    AwaitPayment,
    Active,
    Expired,
    Cancelled
}