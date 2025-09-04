using MassTransit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Test.Application.Dtos.DASS21Recommendations;
using Test.Application.Dtos.Gemini;
using Test.Application.Extensions.Utils;
using Test.Application.ServiceContracts;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services;

public class GeminiClient(
    ILogger<GeminiClient> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    IRequestClient<AggregatePatientProfileRequest> profileClient,
    IRequestClient<AggregatePatientLifestyleRequest> lifestyleClient)
    : IAIClient
{

    public async Task<CreateRecommendationResponseDto> GetDASS21RecommendationsAsync(
        string patientProfileId,
        Score depressionScore,
        Score anxietyScore,
        Score stressScore
    )
    {
        var profileResponse = await profileClient.GetResponse<AggregatePatientProfileResponse>(
            new AggregatePatientProfileRequest(Guid.Parse(patientProfileId)));

        var lifestyleResponse = await lifestyleClient.GetResponse<AggregatePatientLifestyleResponse>(
            new AggregatePatientLifestyleRequest(Guid.Parse(patientProfileId), DateTime.UtcNow));

        var profile = profileResponse.Message;
        var lifestyle = lifestyleResponse.Message;

        var contentParts = new List<GeminiContentDto>();

        var profileNickname = ProfileClassifier.GetNickname(depressionScore, anxietyScore, stressScore);

        var prompt = BuildGeminiDASS21Prompt(profileNickname, depressionScore, anxietyScore, stressScore, profile, lifestyle);

        contentParts.Add(new GeminiContentDto(
            "user", [new GeminiContentPartDto(prompt)]
        ));

        var payload = BuildGeminiMessagePayload(contentParts);

        var responseText = await CallGeminiAPIAsync(payload);
        
        logger.LogInformation("[Gemini API response]: {ResponseText}", responseText);

        var recommendations = JsonConvert.DeserializeObject<RecommendationsDto>(responseText)!;

        logger.LogInformation("\n\n[Parsed recommendations]: {@Recommendations}", recommendations);
        
        var age = DateOnlyUtils.CalculateAge(profile.BirthDate);

        var response = new CreateRecommendationResponseDto(Recommendation: recommendations,
            ProfileDescription: recommendations.ProfileDescription, ProfileNickname: profileNickname,
            PatientName: profile.FullName, PatientAge: age, ProfileHighlights: recommendations.ProfileHighlights);

        return response;
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


    private static string BuildGeminiDASS21Prompt(
        string profileNickname,
        Score depressionScore, Score anxietyScore, Score stressScore,
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

                        ### 📊 Kết quả DASS-21 (raw values, chưa nhân 2)
                        - **Trầm cảm**: {depressionScore.Value}  
                        - **Lo âu**: {anxietyScore.Value}  
                        - **Căng thẳng**: {stressScore.Value}

                        ### 👑 Biệt danh cá nhân hóa
                        Biệt danh (profileNickname) của người dùng này là: **{profileNickname}**.  
                        Dựa trên biệt danh này và các chỉ số DASS-21, hãy:
                        - Sinh ra một mô tả cá tính ngắn gọn (profileDescription) tối đa 2 câu **bằng ngôi thứ 3** (ví dụ: “Những người thuộc nhóm này…”, “Họ thường…”), thể hiện điểm mạnh cảm xúc/tâm lý hoặc cách đối diện áp lực đặc trưng cho nhóm này.
                        - Lưu ý: Mô tả này phải phù hợp biệt danh đã cho và các chỉ số DASS-21, không dùng các câu sáo rỗng, không phán xét, tránh đề cập tới “bệnh”, “rối loạn”.

                        #### ✨ **3 đặc điểm nổi bật nhất (profileHighlights)**
                        Sau khi mô tả cá tính, **liệt kê 3 đặc điểm hoặc điểm mạnh nổi bật nhất của profile này dưới dạng danh sách**, mỗi đặc điểm 1 dòng ngắn gọn **dùng ngôi thứ 3** (ví dụ: “Họ luôn giữ được sự bình tĩnh…”, “Những người thuộc nhóm này rất kiên định…”), tập trung vào tố chất/tài năng/thái độ tích cực mà biệt danh này thể hiện.

                        ---

                        ### 🪞 Tổng quan tâm lý
                        Viết một đoạn phân tích về nhóm tính cách {profileNickname} ở ngôi thứ ba (“Những người thuộc nhóm này…”, “Họ thường…”).
                        Nhấn mạnh tố chất tâm lý nổi bật, ý nghĩa trong sức khỏe tinh thần.
                        Kể chi tiết: Họ thường thể hiện ra sao khi làm việc, sống trong tập thể hoặc ở các độ tuổi khác nhau.
                        Văn phong truyền cảm hứng, khách quan, không chẩn đoán, không lặp lại số liệu.
                        Độ dài tầm 110 từ.

                        ---

                        ### 🧭 Phân tích trạng thái cảm xúc hiện tại
                        Dựa trên các chỉ số DASS-21 và đặc điểm cá nhân, mô tả các trạng thái cảm xúc hoặc thách thức nổi bật mà những cá nhân thuộc nhóm này có thể đang trải qua ở thời điểm hiện tại.  
                        Chú ý liên hệ giữa số liệu (điểm trầm cảm/lo âu/căng thẳng) với biểu hiện thực tiễn trong công việc hoặc cuộc sống.  
                        Không nhắc thẳng tên người dùng, nhưng có thể gọi họ là "bạn" để tăng cảm giác cá nhân hóa.
                        Kết thúc bằng một câu gợi mở về cách để xoa dịu hoặc cải thiện cảm xúc hiện tại để gợi mở cho phần gợi ý tiếp theo.
                        Độ dài tầm 110 từ.
                        
                        {improvementGoalsSection}
                        {recentEmotionsSection}


                        ---

                        ### 🎯 Gợi ý cho bạn
                        Đưa ra **3 hoạt động nhẹ nhàng, cá nhân hóa theo kết quả DASS-21 và đặc điểm người dùng**, mỗi hoạt động gồm:
                        - **Tiêu đề gợi cảm xúc tích cực**.
                        - **Mô tả sâu hơn** (3–4 câu) về lợi ích của hoạt động, lý giải vì sao nó phù hợp với người có mức độ trầm cảm/lo âu/căng thẳng như vậy. Có thể tham chiếu đến nghề nghiệp, tính cách hoặc độ tuổi nếu phù hợp.
                        - **Danh sách 2 hành động cụ thể, dễ thử** mà người đọc có thể bắt đầu ngay từ hôm nay, liên quan tới profile người dùng.
                        - **(reference) Một trích dẫn hoặc dẫn chứng khoa học** có thật, trình bày ngắn gọn, gợi sự tin cậy và dễ hiểu. Ví dụ: “Theo nghiên cứu của Đại học ... năm ..., người dành ... phút mỗi ngày để ... có mức độ lo âu thấp hơn ...%”.
                        - **Gọi người dùng là “bạn” - không nhắc thẳng tên.

                        Lưu ý:
                        - Văn phong **ấm áp – gần gũi – mang tính nâng đỡ**, không mang giọng giảng giải.
                        - **Kết nối gợi ý với kết quả DASS-21 và persona**.
                        - **Markdown** các thông tin đã được cá nhân hóa cho người dùng như tên, tuổi, nghề nghiệp, tính cách, v.v. để tạo cảm giác thân thiện và gần gũi.
                        ---

                        ### 💌 Lời chúc
                        Kết thúc bằng một lời nhắn **tích cực và mạnh mẽ** đến {profile.FullName} (nên gọi bằng tên thay vì cả họ và tên), nhấn mạnh rằng người đọc **xứng đáng được chữa lành và hạnh phúc**, và **không hề đơn độc**.  

                        ---
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
                profileNickname = new { type = "string" }, // Thêm
                profileDescription = new { type = "string" }, // Thêm
                profileHighlights = new
                {
                    type = "array",
                    items = new { type = "string" }, // Danh sách 3 đặc điểm nổi bật
                },

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