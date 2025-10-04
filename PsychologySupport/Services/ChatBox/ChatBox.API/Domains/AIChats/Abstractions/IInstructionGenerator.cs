namespace ChatBox.API.Domains.AIChats.Abstractions;

/// <summary>
/// Service để tạo ra một chỉ dẫn/gợi ý cho AI chính về cách trả lời.
/// Đây là một bước meta-prompting.
/// </summary>
public interface IInstructionGenerator
{
    /// <summary>
    /// Tạo ra một chuỗi chỉ dẫn dựa trên ngữ cảnh cuộc trò chuyện.
    /// </summary>
    /// <param name="userMessage">Tin nhắn hiện tại của người dùng.</param>
    /// <param name="history">Lịch sử tóm tắt hoặc các tin nhắn trước đó.</param>
    /// <param name="persona">Thông tin persona của người dùng.</param>
    /// <returns>Một chuỗi chỉ dẫn, ví dụ: "[Gợi ý trả lời: An ủi và xác thực cảm xúc của người dùng.]".</returns>
    Task<string> GenerateInstructionAsync(string userMessage, string? history = null, string? persona = null);
}