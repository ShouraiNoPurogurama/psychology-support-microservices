using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Pii.Extensions;

namespace Profile.API.Domains.Pii.Features.PatchPii;

public record PatchPiiRequest(UpdatePiiDto Pii);

public record PatchPiiResponse(bool IsSuccess);

public class PatchPiiEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("v1/users",
                async (
                    [FromBody] PatchPiiRequest request,
                    ClaimsPrincipal user,
                    ISender sender) =>
                {
                    var subjectRef = user.GetSubjectRef();
                    
                    var command = new PatchPiiCommand(subjectRef, request.Pii);
                    var result = await sender.Send(command);
                    var response = result.Adapt<PatchPiiResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("PatchPii")
            .WithTags("Pii")
            .Produces<PatchPiiResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Patch Pii")
            .WithSummary("Patch Pii");
    }
}