using Media.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Media.Infrastructure.Options;
using System.Text;
using System.Text.Json;

namespace Media.Infrastructure.Services
{
    public class SightengineService : ISightengineService
    {
        private readonly HttpClient _httpClient;
        private readonly SightengineOptions _options;

        public SightengineService(HttpClient httpClient, IOptions<SightengineOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(_options.ApiUser) || string.IsNullOrEmpty(_options.ApiSecret))
            {
                throw new ArgumentException("Sightengine API credentials are not configured.");
            }
        }

        public async Task<(bool IsSafe, List<string> Violations)> CheckImageAsync(IFormFile file)
        {
            var content = new MultipartFormDataContent
            {
                { new StreamContent(file.OpenReadStream()), "media", file.FileName }
            };

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiUser}:{_options.ApiSecret}")));

            var response = await _httpClient.PostAsync("1.0/check.json?models=properties,discrimination,offensive", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Lỗi khi kiểm tra nội dung với Sightengine API: {response.ReasonPhrase}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SightengineResponse>(jsonResponse) ?? new SightengineResponse();

            var violations = new List<string>();
            if (result.Offensive?.Probability > 0.5) violations.Add("Nội dung nhạy cảm");
            if (result.Discrimination?.Probability > 0.5) violations.Add("Nội dung phân biệt đối xử");
            if (result.Properties?.IsBlurry == true) violations.Add("Ảnh mờ");

            return (violations.Count == 0, violations);
        }

        public async Task<(bool IsSafe, List<string> Violations)> CheckImageWithWorkflowAsync(IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiUser) ||
                string.IsNullOrWhiteSpace(_options.ApiSecret) ||
                string.IsNullOrWhiteSpace(_options.WorkflowId) ||
                string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                throw new ArgumentException("Sightengine API configuration is missing or invalid.");
            }

            var content = new MultipartFormDataContent
            {   
                { new StreamContent(file.OpenReadStream()), "media", file.FileName },
                { new StringContent(_options.ApiUser), "api_user" },
                { new StringContent(_options.ApiSecret), "api_secret" },    
                { new StringContent(_options.WorkflowId), "workflow" } 
            };

            //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            ////_httpClient.DefaultRequestHeaders.Clear(); 
            //_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            //var endpoint = _options.BaseUrl;

            var response = await _httpClient.PostAsync("check-workflow.json", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi kiểm tra workflow với Sightengine API: {response.ReasonPhrase} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SightengineResponse>(jsonResponse) ?? new SightengineResponse();

            var violations = new List<string>();
            if (result.Offensive?.Probability > 0.5) violations.Add("Nội dung nhạy cảm");
            if (result.Discrimination?.Probability > 0.5) violations.Add("Nội dung phân biệt đối xử");
            if (result.Properties?.IsBlurry == true) violations.Add("Ảnh mờ");

            return (violations.Count == 0, violations);
        }
    }

    public class SightengineResponse
    {
        public OffenseData? Offensive { get; set; }
        public DiscriminationData? Discrimination { get; set; }
        public PropertiesData? Properties { get; set; }
    }

    public class OffenseData
    {
        public double Probability { get; set; }
    }

    public class DiscriminationData
    {
        public double Probability { get; set; }
    }

    public class PropertiesData
    {
        public bool IsBlurry { get; set; }
    }
}