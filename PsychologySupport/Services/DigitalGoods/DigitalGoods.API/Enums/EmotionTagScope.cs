using System.Text.Json.Serialization;

namespace DigitalGoods.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmotionTagScope
    {
        Global = 0,   // Emoji dùng chung, không gắn với sản phẩm nào
        Product = 1   // Emoji gắn trực tiếp với DigitalGood
    }
}
