using Google.GenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Shared.Services;

public class GeminiService(IConfiguration configuration, ILogger<GeminiService> logger) : IGeminiService
{
    // Model name từ SDK Google.GenAI thường yêu cầu prefix "models/"
    private const string ModelName = "gemini-2.5-flash-lite"; 

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken ct)
    {
        logger.LogInformation("Calling Gemini API ({ModelName})...", ModelName);

        try
        {
            var apiKey = configuration["GeminiConfig:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogError("Gemini API key is not configured in GeminiConfig:ApiKey");
                throw new InvalidOperationException("Gemini API key is not configured.");
            }

            // 1. Khởi tạo client (như code của bạn)
            var client = new Client(apiKey: apiKey);

            // 2. Thực hiện cuộc gọi
            // SDK này yêu cầu model name là tham số đầu tiên
            var response = await client.Models.GenerateContentAsync(
                model: ModelName,
                contents: prompt
            );

            // 3. Trích xuất văn bản
            var generatedText = response.Candidates[0].Content.Parts[0].Text;

            if (string.IsNullOrEmpty(generatedText))
            {
                // Kiểm tra xem có phải bị block không
                if (response.PromptFeedback?.BlockReason != null)
                {
                    logger.LogWarning(
                        "Gemini request blocked. Reason: {Reason}, SafetyRatings: {Ratings}",
                        response.PromptFeedback.BlockReason,
                        response.PromptFeedback.SafetyRatings);
                    throw new InvalidOperationException($"Gemini request blocked due to: {response.PromptFeedback.BlockReason}");
                }

                logger.LogWarning("Gemini API returned an empty response without block reason.");
                return string.Empty;
            }

            logger.LogInformation("Gemini API call successful.");
            return generatedText;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while calling the Gemini API.");
            throw; // Ném lỗi để (ProcessRewardRequestJob) bắt được và xử lý
        }
    }
}