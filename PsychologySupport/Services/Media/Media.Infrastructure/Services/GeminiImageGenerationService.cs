using System.Text.Json;
using Media.Application.Features.Media.Dtos.Gemini.ImageGeneration;
using Media.Application.Features.Media.Dtos.Gemini.ImageGeneration.Request;
using Media.Application.Features.Media.Dtos.Gemini.ImageGeneration.Response;
using Media.Application.ServiceContracts;
using Media.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Services;

public class GeminiImageGenerationService : IStickerGenerationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiImageGenerationService> _logger;
    private readonly string _apiKey;
    private readonly string _endpointUrl;

    // Cache theo instance, dựa trên config + env
    private readonly Lazy<byte[]> _templateImageBytes;

    public GeminiImageGenerationService(
        IConfiguration configuration,
        ILogger<GeminiImageGenerationService> logger,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment env,
        IOptions<AssetsOptions> assetsOpt)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        _apiKey = configuration["GeminiConfig:ApiKey"]
                  ?? throw new ArgumentNullException("GeminiConfig:ApiKey");

        _endpointUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent";

        _templateImageBytes = new Lazy<byte[]>(() =>
        {
            var rel = assetsOpt.Value.EmoBaseImage;

            // Hỗ trợ cả đường dẫn tuyệt đối lẫn tương đối
            var path = Path.IsPathRooted(rel) ? rel : Path.Combine(env.ContentRootPath, rel);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Không tìm thấy ảnh style template tại: {path}", path);
            }

            return File.ReadAllBytes(path);
        });
    }

    public async Task<Stream> GenerateImageAsync(string prompt, CancellationToken cancellationToken)
    {
        // 1) Đọc file ảnh -> base64
        var base64Template = Convert.ToBase64String(_templateImageBytes.Value);

        // 2) Build payload
        var requestPayload = new GeminiRequest
        {
            Contents =
            [
                new GeminiContentReq
                {
                    Parts =
                    [
                        new GeminiPartReq { Text = prompt },
                        new GeminiPartReq
                        {
                            InlineData = new GeminiInlineDataReq
                            {
                                MimeType = "image/jpeg",
                                Data = base64Template
                            }
                        }
                    ]
                }
            ]
        };

        // 3) Gọi API9
        var httpClient = _httpClientFactory.CreateClient("GeminiApiClient");

        var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl);
        request.Headers.Add("x-goog-api-key", _apiKey);

        var jsonContent = JsonSerializer.Serialize(
            requestPayload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Lỗi API Gemini (HTTP {StatusCode}): {ErrorBody}", response.StatusCode, errorBody);
            throw new ApplicationException($"Lỗi API Gemini: {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);
        // hoặc thêm new JsonSerializerOptions { PropertyNameCaseInsensitive = true }

        var base64Image = geminiResponse?.Candidates?
            .FirstOrDefault()
            ?.Content?.Parts?
            .FirstOrDefault(p => p.InlineData?.Data is not null)?
            .InlineData?.Data;

        if (string.IsNullOrEmpty(base64Image))
        {
            _logger.LogError("Gemini không trả về base64 image data. Response: {JsonResponse}", jsonResponse);
            throw new ApplicationException("Gemini không trả về ảnh trong response.");
        }

        var imageBytes = Convert.FromBase64String(base64Image);
        return new MemoryStream(imageBytes);
    }
}