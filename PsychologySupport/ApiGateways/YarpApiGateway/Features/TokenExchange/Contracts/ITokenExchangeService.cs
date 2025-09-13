namespace YarpApiGateway.Features.TokenExchange.Contracts;

/// <summary>
/// Định nghĩa contract cho dịch vụ dàn xếp quá trình trao đổi token.
/// Nó đóng gói logic phức tạp của việc tra cứu danh tính và tạo token mới.
/// </summary>
public interface ITokenExchangeService
{
    /// <summary>
    /// Thực hiện quá trình trao đổi một token gốc lấy một token nội bộ đã được làm giàu.
    /// </summary>
    /// <param name="originalToken">Chuỗi JWT gốc từ request của client.</param>
    /// <param name="destinationAudience">Audience của dịch vụ đích để đưa vào token mới.</param>
    /// <returns>
    /// Một chuỗi JWT mới nếu trao đổi thành công;
    /// null nếu không thể trao đổi (ví dụ: không tìm thấy mapping).
    /// </returns>
    Task<string?> ExchangeTokenAsync(string originalToken, string destinationAudience);
}