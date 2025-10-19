using System.Text;
using AIModeration.API.Shared.Dtos;
using AIModeration.API.Shared.Dtos.Gemini;
using AIModeration.API.Shared.ServiceContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AIModeration.API.Shared.Services;

public class GeminiClient(
    ILogger<GeminiClient> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration config) : IGeminiClient
{
    public async Task<ModerationResultDto> ModeratePostContentAsync(string postContent)
    {
        // 1. Xây dựng prompt chứa nội dung cần kiểm duyệt
        var prompt = BuildModerationPrompt(postContent);

        // 2. Chuẩn bị content cho request
        var contentParts = new List<GeminiContentDto>
        {
            new GeminiContentDto("user", [new GeminiContentPartDto(prompt)])
        };

        // 3. Xây dựng payload hoàn chỉnh với schema cho structured output
        var payload = BuildGeminiPostContentModerationMessagePayload(contentParts);

        // 4. Gọi API của Gemini
        var responseText = await CallGeminiAPIAsync(payload);

        logger.LogInformation("[Gemini API response]: {ResponseText}", responseText);

        // 5. Deserialize chuỗi JSON trả về thành đối tượng DTO
        var moderationResult = JsonConvert.DeserializeObject<ModerationResultDto>(responseText);

        // 6. Kiểm tra kết quả và trả về
        if (moderationResult is null)
        {
            logger.LogError("Failed to deserialize Gemini API response. Response text: {ResponseText}", responseText);
            throw new InvalidOperationException("Could not parse the moderation result from the Gemini API.");
        }

        return moderationResult;
    }

    public async Task<ModerationResultDto> ModerateAliasLabelAsync(string aliasLabel)
    {
        // 1. Xây dựng prompt chứa nội dung cần kiểm duyệt
        var prompt = BuildModerationPrompt(aliasLabel);

        // 2. Chuẩn bị content cho request
        var contentParts = new List<GeminiContentDto>
        {
            new GeminiContentDto("user", [new GeminiContentPartDto(prompt)])
        };

        // 3. Xây dựng payload hoàn chỉnh với schema cho structured output
        var payload = BuildGeminiAliasModerationPayload(contentParts);

        // 4. Gọi API của Gemini
        var responseText = await CallGeminiAPIAsync(payload);

        logger.LogInformation("[Gemini API response]: {ResponseText}", responseText);

        // 5. Deserialize chuỗi JSON trả về thành đối tượng DTO
        var moderationResult = JsonConvert.DeserializeObject<ModerationResultDto>(responseText);

        // 6. Kiểm tra kết quả và trả về
        if (moderationResult is null)
        {
            logger.LogError("Failed to deserialize Gemini API response. Response text: {ResponseText}", responseText);
            throw new InvalidOperationException("Could not parse the moderation result from the Gemini API.");
        }

        return moderationResult;
    }

    private string BuildModerationPrompt(string postContent)
    {
        return $"""
                Nội dung cần kiểm duyệt:
                "{postContent}"
                """;
    }

    private GeminiRequestDto BuildGeminiPostContentModerationMessagePayload(List<GeminiContentDto> contents)
    {
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                isViolation = new
                {
                    type = "boolean",
                    description = "Trả về 'true' nếu nội dung vi phạm chính sách, ngược lại 'false'."
                },
                categories = new
                {
                    type = "array",
                    description = "Một danh sách các loại vi phạm mà nội dung mắc phải. Trả về mảng rỗng nếu không có vi phạm.",
                    items = new
                    {
                        type = "string",
                        @enum = new[]
                        {
                            "HATE_SPEECH",
                            "VIOLENCE",
                            "SEXUAL_CONTENT",
                            "HARASSMENT",
                            "SELF_HARM",
                            "SPAM_SCAM",
                            "ILLEGAL_ACTIVITIES"
                        }
                    }
                },
                reason = new
                {
                    type = "string",
                    description = "Giải thích ngắn gọn lý do tại sao nội dung bị phân loại là vi phạm."
                },
                confidenceScore = new
                {
                    type = "number",
                    description = "Điểm số từ 0.0 đến 1.0 thể hiện mức độ chắc chắn của quyết định."
                }
            },
            required = new[] { "isViolation", "categories", "reason", "confidenceScore" }
        };

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(
                new GeminiContentPartDto("GeminiConfig:PostModerationSystemInstruction")),
            GenerationConfig: new GeminiGenerationConfigDto(ResponseSchema: responseSchema)
        );
    }

    /// <summary>
    /// Xây dựng payload yêu cầu cho Gemini API để kiểm duyệt tên người dùng (alias).
    /// </summary>
    /// <param name="contents">Nội dung chứa tên người dùng cần kiểm duyệt.</param>
    /// <returns>Một đối tượng GeminiRequestDto đã được cấu hình.</returns>
    private GeminiRequestDto BuildGeminiAliasModerationPayload(List<GeminiContentDto> contents)
    {
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                isViolation = new
                {
                    type = "boolean",
                    description = "Trả về 'true' nếu tên người dùng vi phạm chính sách, ngược lại 'false'."
                },
                categories = new
                {
                    type = "array",
                    description =
                        "Một danh sách các loại vi phạm mà tên người dùng mắc phải. Trả về mảng rỗng nếu không có vi phạm.",
                    items = new
                    {
                        type = "string",
                        // Cập nhật enum theo prompt kiểm duyệt tên người dùng
                        @enum = new[]
                        {
                            "HATE_AND_HARASSMENT",
                            "PROMOTES_SELF_HARM",
                            "NEGATIVE_SELF_TALK",
                            "ILLEGAL_OR_EXPLICIT",
                            "SPAM_IMPERSONATION_PII",
                            "GENERAL_NEGATIVITY"
                        }
                    }
                },
                reason = new
                {
                    type = "string",
                    description = "Giải thích ngắn gọn lý do tại sao tên người dùng bị phân loại là vi phạm."
                },
                confidenceScore = new
                {
                    type = "number",
                    description = "Điểm số từ 0.0 đến 1.0 thể hiện mức độ chắc chắn của quyết định."
                }
            },
            required = new[] { "isViolation", "categories", "reason", "confidenceScore" }
        };

        return new GeminiRequestDto(
            Contents: contents,
            // Sử dụng key instruction mới cho việc kiểm duyệt tên người dùng
            SystemInstruction: new GeminiSystemInstructionDto(
                new GeminiContentPartDto("GeminiConfig:AliasLabelSystemInstruction")),
            GenerationConfig: new GeminiGenerationConfigDto(ResponseSchema: responseSchema)
        );
    }


    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var httpClient = httpClientFactory.CreateClient();
        var apiKey = config["GeminiConfig:ApiKey"];
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite-preview-06-17:generateContent?key={apiKey}";

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