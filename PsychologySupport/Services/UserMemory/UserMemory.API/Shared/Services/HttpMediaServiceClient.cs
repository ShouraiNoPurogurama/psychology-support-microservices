using UserMemory.API.Shared.Dtos;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Shared.Services;

/// <summary>
/// Client HTTP để gọi sang Media Service (Giai đoạn 2)
/// </summary>
public class HttpMediaServiceClient : IMediaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpMediaServiceClient> _logger;

    public HttpMediaServiceClient(
        HttpClient httpClient,
        ILogger<HttpMediaServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MediaGenerationResultDto> GenerateStickerAsync(string prompt, Guid rewardId, CancellationToken ct)
    {
        _logger.LogInformation("Calling Media Service to generate sticker for RewardId: {RewardId}", rewardId);

        // 1. Chuẩn bị request body (Không đổi)
        var request = new GenerateStickerRequest(prompt, rewardId);

        try
        {
            // 2. Gọi API (POST /api/v1/media/generate-sticker)
            // Sửa lại endpoint cho khớp với controller (nếu cần), tạm dùng /v1/media/sticker
            var response = await _httpClient.PostAsJsonAsync("/v1/media/sticker", request, ct);

            response.EnsureSuccessStatusCode(); // Ném lỗi nếu API trả về 4xx, 5xx

            var resultDto = await response.Content.ReadFromJsonAsync<GenerateStickerClientResponse>(cancellationToken: ct);

            // Kiểm tra CdnUrl lồng bên trong
            if (resultDto?.Original?.CdnUrl == null || string.IsNullOrEmpty(resultDto.Original.CdnUrl))
            {
                throw new InvalidOperationException("Media Service returned an invalid or empty response (CdnUrl is missing).");
            }

            _logger.LogInformation("Media Service success for RewardId: {RewardId}. CdnUrl: {CdnUrl}", rewardId, resultDto.Original.CdnUrl);
            
            return new MediaGenerationResultDto(
                CdnUrl: resultDto.Original.CdnUrl, 
                ProviderJobId: string.Empty  
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Media Service for RewardId: {RewardId}. StatusCode: {StatusCode}", rewardId,
                ex.StatusCode);
            throw; // Ném lỗi để Quartz Job bắt được
        }
    }

    private record GenerateStickerRequest(string StickerGenerationPrompt, Guid RewardId);



    private record GenerateStickerClientResponse(
        Guid MediaId,
        string State, 
        MediaVariantClientDto Original
    );

    private record MediaVariantClientDto(
        Guid VariantId,
        string VariantType, 
        string Format,      
        int? Width,
        int? Height,
        string? CdnUrl
    );
}