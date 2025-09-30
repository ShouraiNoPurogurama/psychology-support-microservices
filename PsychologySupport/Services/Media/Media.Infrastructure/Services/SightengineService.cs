using Media.Application.ServiceContracts;
using Media.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Media.Application.Features.Media.Dtos;

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

        public async Task<SightengineResult> CheckImageWithWorkflowAsync(IFormFile file)
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

            var response = await _httpClient.PostAsync($"{_options.BaseUrl}/check-workflow.json", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi kiểm tra workflow với Sightengine API: {response.ReasonPhrase} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var parsed = JsonSerializer.Deserialize<SightengineResponse>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed == null || parsed.Status != "success")
            {
                throw new Exception("Sightengine API trả về dữ liệu không hợp lệ.");
            }

            // Danh sách điểm an toàn (càng cao càng an toàn)
            var safeScores = new List<double>
                {
                    parsed.Nudity?.None ?? 1.0,                                  // tỷ lệ ảnh an toàn (none)
                    1 - (parsed.Nudity?.SexualActivity ?? 0.0),                  // ngược lại với sexual_activity
                    1 - (parsed.Nudity?.SexualDisplay ?? 0.0),                   // ngược lại với sexual_display
                    1 - (parsed.Weapon?.Classes?.Firearm ?? 0.0),                // ngược lại với firearm
                    1 - (parsed.Weapon?.Classes?.Knife ?? 0.0),                  // ngược lại với knife
                    1 - (parsed.Alcohol?.Prob ?? 0.0),                           // ngược lại với alcohol
                    1 - (parsed.Drugs?.Prob ?? 0.0),                             // ngược lại với drugs
                    1 - (parsed.Violence?.Prob ?? 0.0)                           // ngược lại với violence
                };

            // Điểm an toàn cuối cùng là min của tất cả
            var score = safeScores.Min();

            // Đặt ngưỡng an toàn (ví dụ 0.8)
            var isSafe = score > 0.8;

            return new SightengineResult(
                IsSafe: isSafe,
                RawJson: jsonResponse,
                WorkflowId: _options.WorkflowId,
                Score: score
            );
        }
    }
}
