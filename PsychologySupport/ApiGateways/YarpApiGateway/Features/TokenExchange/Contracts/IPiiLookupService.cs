namespace YarpApiGateway.Features.TokenExchange.Contracts;

/// <summary>
/// Định nghĩa contract cho một dịch vụ tra cứu thông tin định danh từ PII service.
/// Contract này có thể được implement bằng gRPC, REST, và có thể được "bọc" bởi một lớp caching.
/// </summary>
public interface IPiiLookupService
{
    /// <summary>
    /// Lấy AliasId tương ứng với một Subject Reference.
    /// </summary>
    /// <param name="subjectRef">SubjectRef từ token của người dùng.</param>
    /// <returns>AliasId dưới dạng chuỗi, hoặc null nếu không tìm thấy.</returns>
    Task<string?> ResolveAliasIdBySubjectRefAsync(string subjectRef);

    /// <summary>
    /// Lấy PatientId tương ứng với một Subject Reference.
    /// </summary>
    /// <param name="subjectRef">SubjectRef từ token của người dùng.</param>
    /// <returns>PatientId dưới dạng chuỗi, hoặc null nếu không tìm thấy.</returns>
    Task<string?> ResolvePatientIdBySubjectRefAsync(string subjectRef);
}