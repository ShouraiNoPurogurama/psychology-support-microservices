using AIModeration.API.Protos;
using AIModeration.API.Shared.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AIModeration.API.Features.Aliases;

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

    public override async Task<ValidateAliasLabelResponse> ValidateAliasLabel(
        ValidateAliasLabelRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Validating alias label: {Label}", request.Label);

            var result = await _moderationService.ModerateAliasLabelAsync(
                request.Label,
                context.CancellationToken);

            var response = new ValidateAliasLabelResponse
            {
                IsValid = result.IsValid,
                PolicyVersion = result.PolicyVersion,
                EvaluatedAt = Timestamp.FromDateTimeOffset(result.EvaluatedAt)
            };

            response.Reasons.AddRange(result.Reasons);

            _logger.LogInformation(
                "Alias label validation completed. Label: {Label}, IsValid: {IsValid}",
                request.Label,
                result.IsValid);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating alias label: {Label}", request.Label);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred during validation"));
        }
    }
}
