using System.Text.Json.Serialization;

namespace DigitalGoods.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConsumptionType
    {
        Consumable,
        Entitlement
    }
}


