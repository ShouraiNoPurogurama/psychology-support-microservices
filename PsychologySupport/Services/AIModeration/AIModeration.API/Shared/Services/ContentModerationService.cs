using AIModeration.API.Models;
using System.Text.RegularExpressions;
using AIModeration.API.Shared.Dtos;
using AIModeration.API.Shared.Services.Contracts;

namespace AIModeration.API.Shared.Services;

/// <summary>
/// Dịch vụ kiểm duyệt nội dung, đánh giá bài đăng và tên hiển thị
/// Sử dụng Gemini AI cho việc kiểm duyệt nội dung bài đăng và xác thực dựa trên quy tắc cho tên hiển thị
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private const string PolicyVersion = "v1.0.0-gemini-ai";
    private const string AliasPolicyVersion = "v1.0.0-gemini-ai";

    private readonly IGeminiClient _geminiClient;
    private readonly ILogger<ContentModerationService> _logger;

    // Các mẫu tên hiển thị bị cấm (ví dụ: tên của quản trị viên)
    private static readonly HashSet<string> ProhibitedAliasPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "moderator", "system", "official", "support", "staff",
        "quản trị", "hỗ trợ", "hệ thống"
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
            // Kết hợp tiêu đề và nội dung để AI có đầy đủ ngữ cảnh
            var combinedContent = string.IsNullOrWhiteSpace(title)
                ? content
                : $"Tiêu đề: {title}\nNội dung: {content}";

            _logger.LogInformation("Moderating post {PostId} with Gemini AI", postId);

            var geminiResult = await _geminiClient.ModeratePostContentAsync(combinedContent);

            string status;
            List<string> reasons = new();

            if (geminiResult.IsViolation)
            {
                // Nếu AI rất chắc chắn (>= 70%), từ chối ngay. Nếu không, gắn cờ để xem xét.
                status = geminiResult.ConfidenceScore >= 0.7 ? "Rejected" : "Flagged";

                // Thêm danh mục vi phạm làm lý do
                if (geminiResult.Categories.Any())
                {
                    reasons.Add($"Danh mục vi phạm: {string.Join(", ", geminiResult.Categories)}");
                }

                // Thêm lý do cụ thể từ AI
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

            // Xử lý lỗi: Tự động gắn cờ bài đăng để xem xét thủ công
            return new PostModerationResultDto
            {
                PostId = postId,
                Status = "Flagged",
                Reasons = new List<string> { "Hệ thống kiểm duyệt AI hiện đang bận. Nội dung của bạn đã được chuyển cho quản trị viên xem xét." },
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

        // --- Các quy tắc kiểm duyệt cơ bản ---

        // Kiểm tra độ dài tối thiểu
        if (string.IsNullOrWhiteSpace(label) || label.Length < 3)
        {
            reasons.Add("Tên hiển thị quá ngắn (cần ít nhất 3 ký tự)");
        }

        // Kiểm tra độ dài tối đa
        if (label.Length > 50)
        {
            reasons.Add("Tên hiển thị quá dài (tối đa 50 ký tự)");
        }

        // Kiểm tra các từ cấm (admin, moderator, v.v.)
        foreach (var pattern in ProhibitedAliasPatterns)
        {
            if (label.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add($"Tên hiển thị chứa cụm từ không được phép: {pattern}");
            }
        }

        // Kiểm tra ký tự đặc biệt (chỉ cho phép chữ, số, khoảng trắng và - _ .)
        if (!Regex.IsMatch(label, @"^[\w\s\-_.]+$"))
        {
            reasons.Add("Tên hiển thị chứa ký tự đặc biệt không hợp lệ");
        }

        // Kiểm tra ký tự lặp lại quá nhiều (ví dụ: "aaaaaa")
        if (Regex.IsMatch(label, @"(.)\1{4,}"))
        {
            reasons.Add("Tên hiển thị chứa các ký tự lặp lại quá nhiều");
        }

        // --- Kiểm duyệt bằng AI ---
        // Nếu đã vi phạm quy tắc cơ bản, có thể không cần gọi AI để tiết kiệm chi phí
        // Tuy nhiên, ở đây chúng ta gọi AI trong mọi trường hợp để bắt các trường hợp tinh vi
        
        try
        {
            _logger.LogInformation("Moderating alias label {AliasLabel} with Gemini AI", label);

            var geminiResult = await _geminiClient.ModerateAliasLabelAsync(label);

            string status;

            if (geminiResult.IsViolation)
            {
                status = geminiResult.ConfidenceScore >= 0.7 ? "Rejected" : "Flagged";

                // Thêm danh mục vi phạm
                if (geminiResult.Categories.Any())
                {
                    reasons.Add($"Danh mục vi phạm: {string.Join(", ", geminiResult.Categories)}");
                }

                // Thêm lý do từ AI
                if (!string.IsNullOrWhiteSpace(geminiResult.Reason))
                {
                    reasons.Add(geminiResult.Reason);
                }
            }
            else
            {
                // AI cho là ổn, nhưng vẫn có thể bị từ chối bởi các quy tắc cơ bản ở trên
                status = "Approved"; 
            }

            _logger.LogInformation(
                "Alias label {AliasLabel} moderation completed. AI Status: {Status}, Confidence: {Confidence}",
                label, status, geminiResult.ConfidenceScore);

            // Tên hiển thị chỉ hợp lệ khi KHÔNG có lý do vi phạm nào (cả từ quy tắc cơ bản và AI)
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

            // Xử lý lỗi: Không thể xác thực tên, yêu cầu người dùng thử lại
            return new AliasLabelModerationResultDto
            {
                IsValid = false,
                Reasons = new List<string> { "Hệ thống kiểm duyệt AI đang bận. Vui lòng thử lại sau giây lát." },
                PolicyVersion = AliasPolicyVersion, // Sửa lỗi (trước đây là PolicyVersion)
                EvaluatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}