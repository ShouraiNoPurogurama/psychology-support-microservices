using System.Security.Claims;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Aliases.Features.IssueAlias;

public record IssueAliasRequest(
    string Label,
    string? ReservationToken = null);

public record IssueAliasResponse(
    Guid AliasId, 
    Guid AliasVersionId, 
    string Label, 
    string Visibility,
    DateTimeOffset CreatedAt);

public class IssueAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/aliases/issue", async (IssueAliasRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                var subjectRef = user.GetSubjectRef();
                
                var command = new IssueAliasCommand(subjectRef, request.ReservationToken, request.Label);
                
                var result = await sender.Send(command);
                
                var response = result.Adapt<IssueAliasResponse>();
                
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("IssueAlias")
            .WithTags("Aliases")
            .Produces<IssueAliasResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Issues a new alias for the authenticated user. Requires authorization. Returns alias details on success.")
            .WithSummary("Issue a new alias");
    }
}