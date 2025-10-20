using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Reactions.Commands.CreateReaction;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.API.Endpoints.Reactions;

public sealed record CreateReactionRequest(
    ReactionTargetType TargetType,
    Guid TargetId,
    ReactionCode ReactionCode
);

public sealed record CreateReactionResponse(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    ReactionCode ReactionCode,
    DateTimeOffset CreatedAt
);

public class CreateReactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/reactions", async (
                CreateReactionRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateReactionCommand(
                    request.TargetType,
                    request.TargetId,
                    request.ReactionCode
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<CreateReactionResponse>();

                return Results.Created($"/v1/reactions/{response.ReactionId}", response);
            })
            .RequireAuthorization()
            .WithTags("Reactions")
            .WithName("CreateReaction")
            .WithSummary("Creates a reaction on a post or comment.")
            .WithDescription("This endpoint allows an authenticated user to react to a post or comment. The target type and ID must be specified, along with a valid reaction code. Duplicate reactions by the same user are prevented. Returns the created reaction details.")
            .Produces<CreateReactionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
