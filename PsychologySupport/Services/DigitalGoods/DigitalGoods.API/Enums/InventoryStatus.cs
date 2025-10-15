using System.Text.Json.Serialization;

namespace DigitalGoods.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InventoryStatus
    {
        Active = 0,   // Còn hiệu lực
        Expired = 1   // Hết hạn
    }
}
