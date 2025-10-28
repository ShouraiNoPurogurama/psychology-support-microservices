using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Utils
{
    /// <summary>
    /// Converter "tolerant" cho string:
    /// - Khi ghi (Write): nếu chuỗi là JSON hợp lệ (object/array/primitive) -> ghi RAW JSON; ngược lại -> ghi string bình thường.
    /// - Khi đọc (Read): nếu token là string -> trả string; nếu token là object/array/primitive -> trả RawText của token.
    /// </summary>
    public sealed class RawJsonStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString();

            using var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            var s = value.AsSpan().Trim();

            if (s.Length == 0)
            {
                writer.WriteStringValue(string.Empty);
                return;
            }

            if (LooksLikeJson(s))
            {
                try
                {
                    using var doc = JsonDocument.Parse(value);
                    doc.RootElement.WriteTo(writer);
                    return;
                }
                catch
                {
                    // fallback xuống ghi string
                }
            }

            writer.WriteStringValue(value);
        }

        private static bool LooksLikeJson(ReadOnlySpan<char> s)
        {
            if (s[0] == '{' || s[0] == '[') return true;
            if (s[0] == 't' || s[0] == 'f' || s[0] == 'n') return true;
            if (s[0] == '-' || char.IsDigit(s[0])) return true;
            if (s[0] == '"' && s.Length >= 2 && s[^1] == '"') return true;
            return false;
        }
    }
}
