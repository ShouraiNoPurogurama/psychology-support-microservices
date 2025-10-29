using AIModeration.API.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AIModeration.API.Shared.Services;

/// <summary>
/// Triển khai dịch vụ gRPC cho chức năng kiểm duyệt.
/// </summary>
public class ModerationGrpcService : ModerationService.ModerationServiceBase
{
    private readonly IContentModerationService _moderationService;
    private readonly ILogger<ModerationGrpcService> _logger;

    public ModerationGrpcService(
        IContentModerationService moderationService,
        ILogger<ModerationGrpcService> logger)
    {
        _moderationService = moderationService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint gRPC để xác thực một tên hiển thị (alias label).
    /// </summary>
    public override async Task<ValidateAliasLabelResponse> ValidateAliasLabel(
        ValidateAliasLabelRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Đang xác thực tên hiển thị: {Label}", request.Label);

            // Gọi service kiểm duyệt lõi
            var result = await _moderationService.ModerateAliasLabelAsync(
                request.Label,
                context.CancellationToken);

            var response = new ValidateAliasLabelResponse
            {
                IsValid = result.IsValid,
                PolicyVersion = result.PolicyVersion,
                EvaluatedAt = Timestamp.FromDateTimeOffset(result.EvaluatedAt)
            };

            // Thêm các lý do (nếu có) vào response
            response.Reasons.AddRange(result.Reasons);

            _logger.LogInformation(
                "Hoàn tất xác thực tên hiển thị. Tên: {Label}, Hợp lệ: {IsValid}",
                request.Label,
                result.IsValid);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đang xác thực tên hiển thị: {Label}", request.Label);
            
            // Ném ra một lỗi gRPC chuẩn để client có thể xử lý
            throw new RpcException(new Status(StatusCode.Internal, "Đã có lỗi hệ thống trong quá trình xác thực. Vui lòng thử lại."));
        }
    }
}