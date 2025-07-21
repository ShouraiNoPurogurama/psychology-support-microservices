using System.Diagnostics;
using System.Text;
using BuildingBlocks.Messaging.Dtos.LifeStyles;
using BuildingBlocks.Messaging.Events.LifeStyle;
using MassTransit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Scheduling.API.Dtos.Gemini;
using Scheduling.API.Utils;

namespace Scheduling.API.Services;

public class GeminiClient(
    ILogger<GeminiClient> logger,
    IHttpClientFactory httpClientFactory,
    IRequestClient<AggregatePatientLifestyleRequest> requestClient,
    IConfiguration config)
{
    public async Task<ShortSchedule> OptimizeScheduleAsync(
        Guid patientProfileId,
        ShortSchedule schedule,
        List<ActivityDto> activities)
    {
        //0. Thu thập thông tin lối sống của người dùng
        var patientLifeStyleResponse = await requestClient.GetResponse<AggregatePatientLifestyleResponse>(
            new AggregatePatientLifestyleRequest(patientProfileId, DateTime.Now));
        
        var lifestyle = patientLifeStyleResponse.Message;
        
        // 1. Build prompt context
        var prompt = BuildScheduleOptimizePrompt(lifestyle, schedule, activities);

        var contentParts = new List<GeminiContentDto>
        {
            new GeminiContentDto("user", [new GeminiContentPartDto(prompt)])
        };

        var payload = BuildGeminiMessagePayload(contentParts);

        var responseText = await CallGeminiAPIAsync(payload);

        logger.LogInformation("[Gemini API response]: {ResponseText}", responseText);

        // 2. Parse output về object structured (nên dùng model giống schema gửi lên)
        var optimizedSchedule = JsonConvert.DeserializeObject<ShortSchedule>(responseText)!;

        return optimizedSchedule;
    }

    private string BuildScheduleOptimizePrompt(AggregatePatientLifestyleResponse lifestyle, ShortSchedule schedule, List<ActivityDto> activities)
    {
        var scheduleJson = JsonConvert.SerializeObject(schedule, Formatting.Indented);
        var activitiesJson = JsonConvert.SerializeObject(activities, Formatting.Indented);
        
        
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

        
        return $"""
                    Bạn là chuyên gia tối ưu lịch trình sức khỏe.

                    {improvementGoalsSection}
                    {recentEmotionsSection}
                
                    Dưới đây là lộ trình hiện tại (JSON):
                    {scheduleJson}

                    Dưới đây là danh sách các activity hợp lệ (JSON):
                    {activitiesJson}

                    Hướng dẫn:
                    - Mỗi activity trong lộ trình đều có trường `Type`, `TimeRange`, `Duration`, `DateNumber`.
                    - Hãy duyệt từng session, từng activity, và nếu thấy activityId, thời điểm, hoặc thời lượng chưa hợp lý thì **chỉ thay bằng Id, TimeRange, Duration, DateNumber của activity khác cùng loại từ danh sách trên**.
                    - Đặc biệt, hãy tối ưu hóa thời điểm thực hiện các activity dựa trên các nguyên lý khoa học về quản lý thời gian như:
                      + **Time-Blocking**: Gom các hoạt động tương tự vào các khung thời gian hợp lý trong ngày.
                      + **Hiệu suất buổi sáng/tối**: Ưu tiên hoạt động thể chất hoặc sáng tạo vào buổi sáng khi năng lượng cao, hoạt động giải trí/thư giãn vào cuối ngày.
                      + **Phù hợp nhịp sinh học cá nhân**: Nếu có thể, hãy gợi ý time range phù hợp với đa số người dùng.
                    - Trong mỗi session, tránh lặp lại activityId giống nhau và cân đối loại activity.
                    - Không được tạo activityId mới, không đổi Type, không thêm/xóa trường nào trong schema.
                    - Đầu ra phải giữ đúng schema `ShortSchedule`, không thay đổi tên/trường.

                    Chỉ trả về object JSON đúng format như lộ trình mẫu.
                """;
    }


    private GeminiRequestDto BuildGeminiMessagePayload(List<GeminiContentDto> contents)
    {
        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                sessions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            order = new { type = "integer" },
                            purpose = new { type = "string" },
                            date = new { type = "string" },
                            activities = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        type = new { type = "string" },
                                        activityId = new { type = "string" }
                                    },
                                    required = new[] { "type", "activityId" }
                                }
                            }
                        },
                        required = new[] { "order", "purpose", "date", "activities" }
                    }
                }
            },
            required = new[] { "sessions" }
        };

        return new GeminiRequestDto(
            Contents: contents,
            SystemInstruction: new GeminiSystemInstructionDto(new GeminiContentPartDto("GeminiConfig:SystemInstruction")),
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