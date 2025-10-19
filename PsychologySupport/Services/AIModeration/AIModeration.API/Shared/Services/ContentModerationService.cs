using AIModeration.API.Models;
using AIModeration.API.Shared.ServiceContracts;
using System.Text.RegularExpressions;
using AIModeration.API.Shared.Dtos;

namespace AIModeration.API.Shared.Services;

/// <summary>
/// Content moderation service that evaluates posts and alias labels
/// Uses Gemini AI for post content moderation and rule-based validation for alias labels
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private const string PolicyVersion = "v1.0.0-gemini-ai";
    private const string AliasPolicyVersion = "v1.0.0-gemini-ai";

    private readonly IGeminiClient _geminiClient;
    private readonly ILogger<ContentModerationService> _logger;

    private static readonly HashSet<string> ProhibitedAliasPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "moderator", "system", "official", "support", "staff"
    };

    public ContentModerationService(
        IGeminiClient geminiClient,
        ILogger<ContentModerationService> logger)
    {
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<PostModerationResultDto> ModeratePostContentAsync(
        Guid postId,
        string? title,
        string content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var combinedContent = string.IsNullOrWhiteSpace(title)
                ? content
                : $"Nội dung: {content}";

            _logger.LogInformation("Moderating post {PostId} with Gemini AI", postId);

            var geminiResult = await _geminiClient.ModeratePostContentAsync(combinedContent);

            string status;
            List<string> reasons = new();

            if (geminiResult.IsViolation)
            {
                status = geminiResult.ConfidenceScore >= 0.7 ? "Rejected" : "Flagged";

                // Add violation categories as reasons
                if (geminiResult.Categories.Any())
                {
                    reasons.Add($"Violation categories: {string.Join(", ", geminiResult.Categories)}");
                }

                // Add the AI's reason
                if (!string.IsNullOrWhiteSpace(geminiResult.Reason))
                {
                    reasons.Add(geminiResult.Reason);
                }
            }
            else
            {
                status = "Approved";
            }

            _logger.LogInformation(
                "Post {PostId} moderation completed. Status: {Status}, Confidence: {Confidence}",
                postId, status, geminiResult.ConfidenceScore);

            return new PostModerationResultDto
            {
                PostId = postId,
                Status = status,
                Reasons = reasons,
                PolicyVersion = PolicyVersion,
                EvaluatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating post {PostId} with Gemini AI", postId);

            // Fallback: flag for manual review if AI fails
            return new PostModerationResultDto
            {
                PostId = postId,
                Status = "Flagged",
                Reasons = new List<string> { "AI moderation service temporarily unavailable - flagged for manual review" },
                PolicyVersion = PolicyVersion,
                EvaluatedAt = DateTimeOffset.UtcNow
            };
        }
    }

    public async Task<AliasLabelModerationResultDto> ModerateAliasLabelAsync(
        string label,
        CancellationToken cancellationToken = default)
    {
        var reasons = new List<string>();

        // Check if label is empty or too short
        if (string.IsNullOrWhiteSpace(label) || label.Length < 3)
        {
            reasons.Add("Label is too short (minimum 3 characters)");
        }

        // Check if label is too long
        if (label.Length > 50)
        {
            reasons.Add("Label is too long (maximum 50 characters)");
        }

        // Check for prohibited patterns
        foreach (var pattern in ProhibitedAliasPatterns)
        {
            if (label.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add($"Contains prohibited pattern: {pattern}");
            }
        }

        // Check for special characters (allow only alphanumeric, spaces, and basic punctuation)
        if (!Regex.IsMatch(label, @"^[\w\s\-_.]+$"))
        {
            reasons.Add("Contains invalid special characters");
        }

        // Check for repeated characters
        if (Regex.IsMatch(label, @"(.)\1{4,}"))
        {
            reasons.Add("Contains excessive repeated characters");
        }

        try
        {
            _logger.LogInformation("Moderating alias label {AliasLabel} with Gemini AI", label);

            var geminiResult = await _geminiClient.ModerateAliasLabelAsync(label);

            string status;

            if (geminiResult.IsViolation)
            {
                status = geminiResult.ConfidenceScore >= 0.7 ? "Rejected" : "Flagged";

                // Add violation categories as reasons
                if (geminiResult.Categories.Any())
                {
                    reasons.Add($"Violation categories: {string.Join(", ", geminiResult.Categories)}");
                }

                // Add the AI's reason
                if (!string.IsNullOrWhiteSpace(geminiResult.Reason))
                {
                    reasons.Add(geminiResult.Reason);
                }
            }
            else
            {
                status = "Approved";
            }

            _logger.LogInformation(
                "Alias label {AliasLabel} moderation completed. Status: {Status}, Confidence: {Confidence}",
                label, status, geminiResult.ConfidenceScore);

            bool isValid = reasons.Count == 0;

            var result = new AliasLabelModerationResultDto
            {
                IsValid = isValid,
                Reasons = reasons,
                PolicyVersion = AliasPolicyVersion,
                EvaluatedAt = DateTimeOffset.UtcNow
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating alias label {AliasLabel} with Gemini AI", label);

            return new AliasLabelModerationResultDto
            {
                IsValid = false,
                Reasons = new List<string> { "AI moderation service temporarily unavailable - flagged for manual review" },
                PolicyVersion = PolicyVersion,
                EvaluatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}