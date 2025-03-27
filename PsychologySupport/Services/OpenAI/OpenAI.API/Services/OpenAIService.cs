using System.Text.Json;
using System.Text;

namespace OpenAI.API.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing");
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GeneratePlanAsync(string scheduleJson)
        {
            var prompt = $"Dựa trên thông tin lịch trình sau, hãy tạo kế hoạch sức khỏe trong 14 ngày:\n\n{scheduleJson}\n\nTrả về kết quả dưới dạng JSON.";

            var requestBody = new
            {
                model = "gpt-4-turbo",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.7
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
