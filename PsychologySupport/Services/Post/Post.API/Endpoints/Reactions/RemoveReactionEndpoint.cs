using Carter;
using Mapster;
using MediatR;
using Post.Application.Aggregates.Reactions.Commands.RemoveReaction;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.API.Endpoints.Reactions;

public sealed record RemoveReactionRequest(
    ReactionTargetType TargetType,
    Guid TargetId
);

public sealed record RemoveReactionResponse(
    bool WasRemoved,
    DateTimeOffset RemovedAt
);

public class RemoveReactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/v1/reactions", async (
                [AsParameters] RemoveReactionRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new RemoveReactionCommand(
                    request.TargetType,
                    request.TargetId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RemoveReactionResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Reactions")
            .WithName("RemoveReaction")
            .WithSummary("Removes a user's reaction from a post or comment.")
            .WithDescription("This endpoint allows an authenticated user to remove their reaction from a post or comment. The target type and ID must be specified. Returns whether the reaction was removed and the timestamp. Only the user who created the reaction can remove it.")
            .Produces<RemoveReactionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
