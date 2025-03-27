using System.Text.Json;
using System.Text;
using OpenAI.API.Utils;
using OpenAI.Chat;

namespace OpenAI.API.Services
{
    public class OpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            var apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("API Key is missing");
            _configuration = configuration;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GeneratePlanAsync(string scheduleJson)
        {
            var clusteringPattern = await File.ReadAllTextAsync("clustering_patterns.csv");
            var clusters = await File.ReadAllTextAsync("clusters.json");
            
            var prompt = PromptUtils.GetCreateScheduleTemplate(scheduleJson, clusteringPattern, clusters);

            ChatClient client = new(
                model: "gpt-4o-mini",
                apiKey: _configuration["OpenAI:ApiKey"]
            );

            ChatCompletion completion = await client.CompleteChatAsync(prompt);
            var responseString = completion.Content[0].Text;

            
            return responseString;
            // Console.WriteLine($"[ASSISTANT]: {responseString}");
            //
            // try
            // {
            //     return JsonDocument.Parse(responseString);
            // }
            // catch (JsonException)
            // {
            //     throw new Exception("Invalid JSON response from OpenAI.");
            // }
        }
    }
}