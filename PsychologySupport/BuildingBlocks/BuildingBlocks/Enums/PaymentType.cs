using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentType
{
    BuySubscription,
    Booking,
    UpgradeSubscription,
    Order,
    BuyPointPackage
}