using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Utils
{
    /// <summary>
    /// Serialize string (đã là JSON) ra raw JSON object/array thay vì chuỗi.
    /// - Write: Parse string và ghi RootElement trực tiếp vào writer.
    /// - Read: Đọc JSON và trả về nguyên văn (GetRawText) để map ngược về string.
    /// </summary>
    public sealed class RawJsonStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                writer.WriteNullValue();
                return;
            }

            using var doc = JsonDocument.Parse(value);
            doc.RootElement.WriteTo(writer); // ghi RAW JSON, không thêm dấu "
        }
    }
}