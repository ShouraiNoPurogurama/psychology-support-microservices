using System.Security.Claims;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Domains.Aliases.Features.IssueAlias;

public record IssueAliasRequest(
    string AliasLabel,
    string ReservationToken);
public record IssueAliasResponse(Guid AliasId, Guid AliasVersionId, string Label, string Visibility);

public class IssueAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/aliases/issue", async (IssueAliasRequest req, ClaimsPrincipal user, ISender sender) =>
            {
                var subjectRef = user.GetSubjectRef();
                
                var cmd = new IssueAliasCommand(subjectRef, req.ReservationToken, req.AliasLabel);
                
                var result = await sender.Send(cmd);
                
                var response = result.Adapt<IssueAliasResponse>();
                
                return Results.Ok(response);
            })
            .RequireAuthorization();
    }
}