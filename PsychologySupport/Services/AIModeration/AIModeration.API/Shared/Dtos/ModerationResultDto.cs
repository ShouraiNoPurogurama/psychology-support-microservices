using System.Text.Json.Serialization;

namespace AIModeration.API.Shared.Dtos;

/// <summary>
/// Đại diện cho kết quả kiểm duyệt nội dung trả về từ Gemini API.
/// Cấu trúc của class này khớp hoàn toàn với responseSchema đã được định nghĩa.
/// </summary>
public class ModerationResultDto
{
    /// <summary>
    /// Cho biết nội dung có vi phạm chính sách hay không.
    /// Ánh xạ từ thuộc tính JSON: "isViolation"
    /// </summary>
    [JsonPropertyName("isViolation")]
    public bool IsViolation { get; set; }

    /// <summary>
    /// Danh sách các loại vi phạm mà nội dung mắc phải.
    /// Sẽ là một danh sách rỗng nếu không có vi phạm.
    /// Ánh xạ từ thuộc tính JSON: "categories"
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();

    /// <summary>
    /// Giải thích ngắn gọn lý do tại sao nội dung bị phân loại là vi phạm.
    /// Ánh xạ từ thuộc tính JSON: "reason"
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    /// <summary>
    /// Điểm số từ 0.0 đến 1.0 thể hiện mức độ chắc chắn của quyết định.
    /// Ánh xạ từ thuộc tính JSON: "confidenceScore"
    /// </summary>
    [JsonPropertyName("confidenceScore")]
    public double ConfidenceScore { get; set; }
}