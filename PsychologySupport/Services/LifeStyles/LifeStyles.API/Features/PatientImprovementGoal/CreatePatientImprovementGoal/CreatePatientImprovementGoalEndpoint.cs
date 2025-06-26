using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientImprovementGoal.CreatePatientImprovementGoal;

public record CreatePatientImprovementGoalRequest(Guid PatientProfileId, List<Guid> GoalIds);

public record CreatePatientImprovementGoalResponse(Guid PatientProfileId, List<Guid> GoalIds);

public class CreatePatientImprovementGoalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/patient-improvement-goals", 
            async (HttpContext httpContext,[FromBody] CreatePatientImprovementGoalRequest request,ISender sender) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                return Results.Forbid();

            var command = new CreatePatientImprovementGoalCommand(
                request.PatientProfileId,
                request.GoalIds);

            await sender.Send(command);

            var response = new CreatePatientImprovementGoalResponse(
                request.PatientProfileId,
                request.GoalIds);

            return Results.Created($"/patient-improvement-goals/{request.PatientProfileId}", response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientImprovementGoals")
            .WithTags("Patient Improvement Goals")
            .WithSummary("Assign improvement goals to a patient")
            .WithDescription("Creates patient-improvement goal assignments")
            .Produces<CreatePatientImprovementGoalResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
