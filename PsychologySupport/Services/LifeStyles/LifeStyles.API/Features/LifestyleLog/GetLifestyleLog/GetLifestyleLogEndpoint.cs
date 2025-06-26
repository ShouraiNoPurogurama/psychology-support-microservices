using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.LifestyleLog.GetLifestyleLog;

public record GetLifestyleLogRequest(Guid PatientProfileId);

public record GetLifestyleLogResponse(LifestyleLogDto LifestyleLog);

public class GetLifestyleLogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/lifestyle-logs/{patientProfileId:guid}",
                async (HttpContext httpContext,Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        return Results.Forbid();

                    var query = new GetLifestyleLogQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetLifestyleLogResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetLifestyleLog")
            .WithTags("LifestyleLogs")
            .WithSummary("Get most recent Lifestyle Log")
            .WithDescription("GetLifestyleLog")
            .Produces<GetLifestyleLogResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
