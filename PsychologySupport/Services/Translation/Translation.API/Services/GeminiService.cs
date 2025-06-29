using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Translation.API.Dtos.Gemini;

namespace Translation.API.Services;

public class GeminiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    /// <summary>
    /// Dịch hàng loạt TextKey kèm nội dung tiếng Anh → trả về bản dịch tiếng Việt
    /// </summary>
    public async Task<Dictionary<string, string>> TranslateKeysAsync(Dictionary<string, string> textKeyToEnglish)
    {
        if (textKeyToEnglish is null || textKeyToEnglish.Count == 0)
            return new();

        var keys = textKeyToEnglish.Keys.ToList();

        var prompt = BuildTranslationPrompt(textKeyToEnglish);

        var content = new GeminiContentDto("user", [new GeminiContentPartDto(prompt)]);

        var payload = new GeminiRequestDto(
            Contents: [content],
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto(
                "Bạn là trình dịch thuật tiếng Anh sang tiếng Việt chuyên nghiệp. Dịch ngắn gọn, dễ hiểu, đúng ngữ cảnh, không thêm chú thích."
            )),
            GenerationConfig: new GeminiGenerationConfigDto(ResponseSchema: BuildTranslationSchema(keys))
        );

        var responseText = await CallGeminiAPIAsync(payload);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText)!;
    }

    /// <summary>
    /// Tạo prompt từ cặp key-value (TextKey: English)
    /// </summary>
    private string BuildTranslationPrompt(Dictionary<string, string> keyToValue)
    {
        var list = string.Join("\n", keyToValue.Select(kv => $"- {kv.Key}: {kv.Value}"));
        return $"""
            Bạn là trình dịch tiếng Anh sang tiếng Việt chuyên nghiệp cho ứng dụng doanh nghiệp.

            Dưới đây là danh sách các cặp khóa và giá trị cần dịch (key là định danh, value là nội dung tiếng Anh). 
            Hãy dịch phần value sang tiếng Việt, giữ nguyên key trong kết quả.

            {list}

            Trả về JSON object với dạng: "TextKey": "Bản dịch tiếng Việt"
        """;
    }

    /// <summary>
    /// Tạo schema Gemini structured output cho các TextKey
    /// </summary>
    private object BuildTranslationSchema(IEnumerable<string> keys)
    {
        var properties = keys.ToDictionary(
            key => key,
            _ => new { type = "string" }
        );

        return new
        {
            type = "object",
            properties = properties,
            required = keys.ToArray()
        };
    }

    /// <summary>
    /// Gọi Gemini API
    /// </summary>
    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var apiKey = _config["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={apiKey}";

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload, settings), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API call failed: {response.StatusCode}\n{result}");

        var jObject = JObject.Parse(result);

        var parts = jObject["candidates"]
            ?.Select(c => c["content"]?["parts"]?[0]?["text"]?.ToString())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        var responseText = string.Join("", parts ?? []);
        return responseText;
    }
}
