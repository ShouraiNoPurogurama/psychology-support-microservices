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

    public async Task<string> GetDASS21RecommendationsAsync(
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


        return responseText.Trim();
    }

    private async Task<string> CallGeminiAPIAsync(GeminiRequestDto payload)
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        var token = await GetGoogleAccessTokenAsync();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_config["GeminiConfig:ApiKey"]}";
        
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
               **🎯 Mục tiêu cải thiện hiện tại:**
               {string.Join("\n", lifestyle.ImprovementGoals.Select(g => $"- {g.GoalName} (giao lúc {g.AssignedAt:yyyy-MM-dd})"))}

               """
            : string.Empty;

        var recentEmotionsSection = lifestyle.EmotionSelections.Any()
            ? $"""
               **🧠 Cảm xúc gần nhất:**
               {string.Join("\n", lifestyle.EmotionSelections.Select(e => $"- {e.EmotionName} "))}

               """
            : string.Empty;
        
        
        var prompt = $"""
                      ## 🌿 Gợi ý cải thiện tâm lý cho **{profile.FullName}**

                      **🧾 Thông tin bệnh nhân:**
                      - Họ tên: {profile.FullName}
                      - Giới tính: {profile.Gender}
                      - Ngày sinh: {profile.BirthDate:yyyy-MM-dd}
                      - Nghề nghiệp: {profile.JobTitle}
                      - Trình độ học vấn: {profile.EducationLevel}
                      - Ngành nghề: {profile.IndustryName}

                      {improvementGoalsSection}{recentEmotionsSection}**📈 Điểm DASS-21:**
                      - Trầm cảm: {depressionScore.Value}
                      - Lo âu: {anxietyScore.Value}
                      - Căng thẳng: {stressScore.Value}

                      ---

                      📊 **Đánh giá tổng quan**  
                      Viết đoạn giới thiệu thân thiện, cảm thông. Đưa ra nhận định ngắn gọn nhưng sâu sắc về trạng thái tâm lý tổng thể dựa trên DASS-21 và cảm xúc gần đây. Có thể sử dụng định dạng > blockquote để tăng tính nhẹ nhàng.

                      ---

                      🧠 **Phân tích cảm xúc**  
                      Nêu ra cách các cảm xúc và mục tiêu hiện tại ảnh hưởng đến sức khỏe tâm thần của người dùng. Liệt kê cụ thể nguyên nhân tiềm năng và hệ quả cảm xúc (ví dụ: thiếu ngủ → dễ cáu gắt, khó tập trung...). Dùng dấu gạch đầu dòng * hoặc > để trình bày rõ ràng.

                      ---

                      🎯 **3 Gợi ý cải thiện cá nhân hóa**  
                      Viết 3 lời khuyên dưới dạng tiêu đề truyền cảm hứng, mỗi lời khuyên bắt đầu bằng emoji và tiêu đề sáng tạo, sau đó là mô tả ngắn cụ thể, dễ áp dụng.

                      ---

                      💌 **Lời chúc cuối**  
                      Đưa ra một lời chúc hoặc nhắn gửi truyền hy vọng, nhẹ nhàng như một lời động viên cuối bài.

                      => Chỉ trả về kết quả:
                      """;

        return prompt;
    }

    private GeminiRequestDto BuildGeminiMessagePayload(List<GeminiContentDto> contents)
    {
        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto("GeminiConfig:SystemInstruction")),
            GenerationConfig: new GeminiGenerationConfigDto(
                Temperature: 1.0,
                TopP: 0.96,
                MaxOutputTokens: 8192
            ),
            SafetySettings:
            [
                new("HARM_CATEGORY_HATE_SPEECH"),
                new("HARM_CATEGORY_DANGEROUS_CONTENT"),
                new("HARM_CATEGORY_SEXUALLY_EXPLICIT"),
                new("HARM_CATEGORY_HARASSMENT")
            ]
        );
    }
    
    private async Task<string> GetGoogleAccessTokenAsync()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped(new[]
        {
            "https://www.googleapis.com/auth/generative-language",
            "https://www.googleapis.com/auth/generative-language.tuning",
            "https://www.googleapis.com/auth/generative-language.tuning.readonly",
            "https://www.googleapis.com/auth/generative-language.retriever",
            "https://www.googleapis.com/auth/generative-language.retriever.readonly"
        });
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}