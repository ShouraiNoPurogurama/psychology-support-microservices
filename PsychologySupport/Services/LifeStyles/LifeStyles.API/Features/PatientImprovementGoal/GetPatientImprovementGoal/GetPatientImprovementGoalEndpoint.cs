using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.PatientImprovementGoal.GetPatientImprovementGoal;

public record GetPatientImprovementGoalRequest(Guid PatientProfileId, DateOnly Date); // clean sau 

public record GetPatientImprovementGoalResponse(IEnumerable<Dtos.PatientImprovementGoalDto> Goals);

public class GetPatientImprovementGoalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-improvement-goals/{patientProfileId:guid}",
            async (HttpContext httpContext,Guid patientProfileId, [AsParameters] QueryDate query, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var queryObj = new GetPatientImprovementGoalQuery(patientProfileId, query.Date);
                var result = await sender.Send(queryObj);
                var response = result.Adapt<GetPatientImprovementGoalResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientImprovementGoals")
            .WithTags("Patient Improvement Goals")
            .WithSummary("GetPatientImprovementGoals")
            .WithDescription("GetPatientImprovementGoals")
            .Produces<GetPatientImprovementGoalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public record QueryDate(DateOnly Date);
}
