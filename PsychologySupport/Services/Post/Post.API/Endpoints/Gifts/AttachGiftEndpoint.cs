using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Gifts.Commands.AttachGift;
using Post.Domain.Aggregates.Gifts.Enums;

namespace Post.API.Endpoints.Gifts;

public sealed record AttachGiftRequest(
    GiftTargetType TargetType,
    Guid TargetId,
    Guid GiftId,
    int Quantity,
    string? Message
);

public sealed record AttachGiftResponse(
    Guid GiftAttachId,
    GiftTargetType TargetType,
    Guid TargetId,
    Guid GiftId,
    string? Message,
    DateTimeOffset CreatedAt
);

public class AttachGiftEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/gifts", async (
                AttachGiftRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new AttachGiftCommand(
                    request.TargetType,
                    request.TargetId,
                    request.GiftId,
                    request.Quantity,
                    request.Message
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<AttachGiftResponse>();

                return Results.Created($"/v1/gifts/{response.GiftAttachId}", response);
            })
            .RequireAuthorization()
            .WithTags("Gifts")
            .WithName("AttachGift")
            .WithSummary("Attaches a gift to a post or comment.")
            .WithDescription("This endpoint allows an authenticated user to attach a gift to a post. The target type and ID, gift ID, quantity, and optional message must be provided. Returns the gift attachment details and timestamp.")
            .Produces<AttachGiftResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
