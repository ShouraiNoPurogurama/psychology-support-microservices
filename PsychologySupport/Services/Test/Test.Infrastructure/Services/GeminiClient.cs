using BuildingBlocks.Messaging.Events.LifeStyle;
using BuildingBlocks.Messaging.Events.Profile;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Test.Application.Dtos.DASS21Recommendations;
using Test.Application.Dtos.Gemini;
using Test.Application.ServiceContracts;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services;

public class GeminiClient : IAIClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IRequestClient<AggregatePatientProfileRequest> _profileClient;
    private readonly IRequestClient<AggregatePatientLifestyleRequest> _lifestyleClient;

    public GeminiClient(IHttpClientFactory httpClientFactory, IConfiguration config,
        IRequestClient<AggregatePatientProfileRequest> profileClient,
        IRequestClient<AggregatePatientLifestyleRequest> lifestyleClient)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _profileClient = profileClient;
        _lifestyleClient = lifestyleClient;
    }

    public async Task<RecommendationsDto> GetDASS21RecommendationsAsync(
        string patientProfileId,
        Score depressionScore,
        Score anxietyScore,
        Score stressScore
    )
    {
        var profileResponse = await _profileClient.GetResponse<AggregatePatientProfileResponse>(
            new AggregatePatientProfileRequest(Guid.Parse(patientProfileId)));

        var lifestyleResponse = await _lifestyleClient.GetResponse<AggregatePatientLifestyleResponse>(
            new AggregatePatientLifestyleRequest(Guid.Parse(patientProfileId), DateTime.UtcNow));

        var profile = profileResponse.Message;
        var lifestyle = lifestyleResponse.Message;

        var contentParts = new List<GeminiContentDto>();

        var prompt = BuildGeminiDASS21Prompt(depressionScore, anxietyScore, stressScore, profile, lifestyle);

        contentParts.Add(new GeminiContentDto(
            "user", [new GeminiContentPartDto(prompt)]
        ));

        var payload = BuildGeminiMessagePayload(contentParts);

        var responseText = await CallGeminiAPIAsync(payload);

        var recommendations = JsonConvert.DeserializeObject<RecommendationsDto>(responseText)!;

        return recommendations;
    }

    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var apiKey = _config["GeminiConfig:ApiKey"];
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite-preview-06-17:generateContent?key={apiKey}";

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


    private static string BuildGeminiDASS21Prompt(Score depressionScore, Score anxietyScore, Score stressScore,
        AggregatePatientProfileResponse profile, AggregatePatientLifestyleResponse lifestyle)
    {
        var improvementGoalsSection = lifestyle.ImprovementGoals.Any()
            ? $"""
               - **Mục tiêu hiện tại**:  
                 {string.Join(", ", lifestyle.ImprovementGoals.Select(g => g.GoalName))}
               """
            : string.Empty;

        var recentEmotionsSection = lifestyle.EmotionSelections.Any()
            ? $"""
               - **Cảm xúc gần đây**:  
                 {string.Join(", ", lifestyle.EmotionSelections.Select(e => e.EmotionName))}
               """
            : string.Empty;

        var prompt = $"""
                      ## 🌿 Gợi ý cải thiện tâm lý cho {profile.FullName}

                      ### 👤 Thông tin người dùng
                      - **Họ tên**: {profile.FullName}  
                      - **Giới tính**: {profile.Gender}  
                      - **Ngày sinh**: {profile.BirthDate:yyyy-MM-dd}  
                      - **Nghề nghiệp**: {profile.JobTitle}  
                      - **Trình độ học vấn**: {profile.EducationLevel}  
                      - **Ngành nghề**: {profile.IndustryName}  
                      - **Tính cách nổi bật**: {profile.PersonalityTraits}  
                      - **Tiền sử dị ứng**: {(string.IsNullOrEmpty(profile.Allergies) ? "Không rõ" : profile.Allergies)}

                      ### 📊 Kết quả DASS-21
                      - **Trầm cảm**: {depressionScore.Value}  
                      - **Lo âu**: {anxietyScore.Value}  
                      - **Căng thẳng**: {stressScore.Value}

                      ### 📖 Đánh giá nhanh
                      Viết một đoạn chào hỏi thân thiện, ngắn gọn. Sau đó, diễn giải kết quả DASS-21 một cách đơn giản, tập trung vào việc đây là trạng thái **tạm thời** và có thể cải thiện.  
                      Giọng văn **nhẹ nhàng, truyền cảm hứng, không phán xét, không chẩn đoán.**

                      ---

                      ### 🧠 Cảm xúc của bạn
                      Mô tả rất ngắn gọn rằng người đọc có thể đang trải qua các cảm xúc như **mệt mỏi, nhạy cảm hoặc không rõ ràng**, và nhấn mạnh rằng đây là điều **hoàn toàn bình thường**.  
                      Tránh phân tích sâu hay suy đoán cụ thể. Giọng văn **trung lập, gợi mở.**  

                      {improvementGoalsSection}
                      {recentEmotionsSection}

                      ---

                      ### 🎯 Gợi ý cho bạn
                      Đưa ra **3 hoạt động nhẹ nhàng, cá nhân hóa theo kết quả DASS-21 và đặc điểm người dùng**, mỗi hoạt động gồm:
                      - **Tiêu đề gợi cảm xúc tích cực**.
                      - **Mô tả sâu hơn** (3–4 câu) về lợi ích của hoạt động, lý giải vì sao nó phù hợp với người có mức độ trầm cảm/lo âu/căng thẳng như vậy. Có thể tham chiếu đến nghề nghiệp, tính cách hoặc độ tuổi nếu phù hợp.
                      - **Danh sách 2 hành động cụ thể, dễ thử** mà người đọc có thể bắt đầu ngay từ hôm nay, liên quan tới profile người dùng.
                      - **Một trích dẫn hoặc dẫn chứng khoa học** có thật, trình bày ngắn gọn, gợi sự tin cậy và dễ hiểu. Ví dụ: “Theo nghiên cứu của Đại học Stanford năm 2019, người dành 30 phút mỗi ngày trong thiên nhiên có mức độ lo âu thấp hơn 21%”.

                      Lưu ý:
                      - Văn phong **ấm áp – gần gũi – mang tính nâng đỡ**, không mang giọng giảng giải.
                      - **Kết nối gợi ý với kết quả DASS-21 và persona** (ví dụ: người hướng nội, công việc áp lực cao, học vấn cao sẽ thích hợp với thiền, âm nhạc, ghi chép...).

                      ---

                      ### 💌 Lời chúc
                      Kết thúc bằng một lời nhắn **tích cực và mạnh mẽ**, nhấn mạnh rằng người đọc **xứng đáng được chữa lành và hạnh phúc**, và **không hề đơn độc**.  
                      Luôn kết bằng chữ ký:  
                      **— Emo 🌿**
                      """;


        return prompt;
    }


    private GeminiRequestDto BuildGeminiMessagePayload(List<GeminiContentDto> contents)
    {
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                overview = new { type = "string" }, // "Đánh giá nhanh"
                emotionAnalysis = new { type = "string" }, // "Cảm xúc của bạn"
                personalizedSuggestions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            title = new { type = "string" }, // Tên gợi ý (ví dụ: "Khu Vườn An Yên")
                            description = new { type = "string" }, // Mô tả ngắn (1-2 câu)
                            tips = new
                            {
                                type = "array",
                                items = new { type = "string" } // 2 hành động cụ thể
                            },
                            reference = new { type = "string" } // Dẫn chứng/trích dẫn khoa học ngắn
                        },
                        required = new[] { "title", "description", "tips", "reference" }
                    }
                },
                closing = new { type = "string" } // "Lời chúc"
            },
            required = new[] { "overview", "emotionAnalysis", "personalizedSuggestions", "closing" },
            propertyOrdering = new[]
                { "overview", "emotionAnalysis", "personalizedSuggestions", "closing" } // Đảm bảo thứ tự phản hồi
        };

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto("GeminiConfig:SystemInstruction")),
            GenerationConfig: new GeminiGenerationConfigDto(ResponseSchema: responseSchema)
        );
    }
}