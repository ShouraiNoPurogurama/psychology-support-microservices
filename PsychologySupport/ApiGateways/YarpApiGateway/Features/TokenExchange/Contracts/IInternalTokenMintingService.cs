using System.Security.Claims;

namespace YarpApiGateway.Features.TokenExchange.Contracts;

/// <summary>
/// Định nghĩa contract cho một dịch vụ chịu trách nhiệm tạo (mint) các token nội bộ.
/// Dịch vụ này là chung chung và không chứa logic nghiệp vụ cụ thể.
/// </summary>
public interface IInternalTokenMintingService
{
    /// <summary>
    /// Tạo ra một access token mới có phạm vi hẹp, đã được làm giàu với các claim bổ sung.
    /// </summary>
    /// <param name="originalPrincipal">Đối tượng ClaimsPrincipal từ token gốc của người dùng.</param>
    /// <param name="additionalClaims">Một danh sách các claim nghiệp vụ (ví dụ: aliasId, patientId) để thêm vào token mới.</param>
    /// <param name="audience">Định danh (audience) của microservice đích mà token này dành cho.</param>
    /// <returns>Một chuỗi JWT đã được ký.</returns>
    string MintScopedToken(
        ClaimsPrincipal originalPrincipal,
        IEnumerable<Claim> additionalClaims,
        string audience);
}