using System.Security.Claims;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.RenameAlias;

public record RenameAliasRequest(string NewLabel);

public record RenameAliasResponse(
    Guid AliasId,
    Guid AliasVersionId,
    string Label,
    string Visibility);

public class RenameAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/aliases/rename", async (RenameAliasRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                var subjectRef = user.GetSubjectRef();

                var cmd = new RenameAliasCommand(subjectRef, req.NewLabel);

                var result = await sender.Send(cmd);

                var response = result.Adapt<RenameAliasResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("RenameAlias")
            .WithTags("Aliases")
            .Produces<RenameAliasResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Renames an existing alias for the authenticated user. Requires authorization. Returns updated alias details on success.")
            .WithSummary("Rename an alias");
    }
}