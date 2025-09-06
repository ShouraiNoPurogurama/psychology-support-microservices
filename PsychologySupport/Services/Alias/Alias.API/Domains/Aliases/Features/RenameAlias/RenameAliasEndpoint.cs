using System.Security.Claims;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Domains.Aliases.Features.RenameAlias;

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
            .RequireAuthorization();
    }
}