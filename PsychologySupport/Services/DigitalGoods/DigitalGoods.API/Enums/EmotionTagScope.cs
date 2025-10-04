using System.Text.Json.Serialization;

namespace DigitalGoods.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmotionTagScope
    {
        Free = 0,   // Emoji dùng chung, không gắn với sản phẩm nào
        Paid = 1   // Emoji gắn trực tiếp với DigitalGood
    }
}
