using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserMemory.API.Domains.Rewards.Features.ClaimReward;

public record ClaimRewardResponse(Guid RewardId, string StickerGenerationJobStatus);

public class ClaimRewardEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("v1/rewards/claim", async (
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            Guid chatSessionId,
            ILogger<ClaimRewardEndpoint> logger, ISender sender) =>
        {
            if (requestKey is null || requestKey == Guid.Empty)
            {
                logger.LogWarning("Missing or invalid Idempotency-Key header.");
                throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                    "MISSING_IDEMPOTENCY_KEY");
            }

            var result = await sender.Send(new ClaimRewardCommand(requestKey.Value, chatSessionId));

            var response = result.Adapt<ClaimRewardResponse>();

            return Results.Ok(response);
        });
    }
}