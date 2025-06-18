using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientImprovementGoal.CreatePatientImprovementGoal;

public record CreatePatientImprovementGoalRequest(Guid PatientProfileId, List<Guid> GoalIds);

public record CreatePatientImprovementGoalResponse(Guid PatientProfileId, List<Guid> GoalIds);

public class CreatePatientImprovementGoalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/patient-improvement-goals", async (
                [FromBody] CreatePatientImprovementGoalRequest request,
                ISender sender) =>
        {
            var command = new CreatePatientImprovementGoalCommand(
                request.PatientProfileId,
                request.GoalIds);

            await sender.Send(command);

            var response = new CreatePatientImprovementGoalResponse(
                request.PatientProfileId,
                request.GoalIds);

            return Results.Created($"/patient-improvement-goals/{request.PatientProfileId}", response);
        })
            .WithName("CreatePatientImprovementGoals")
            .WithTags("Patient Improvement Goals")
            .WithSummary("Assign improvement goals to a patient")
            .WithDescription("Creates patient-improvement goal assignments")
            .Produces<CreatePatientImprovementGoalResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
