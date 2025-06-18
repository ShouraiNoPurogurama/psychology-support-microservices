using Carter;
using MediatR;
using Mapster;

namespace LifeStyles.API.Features.PatientImprovementGoal.GetPatientImprovementGoal;

public record GetPatientImprovementGoalRequest(Guid PatientProfileId, DateTime Date);

public record GetPatientImprovementGoalResponse(IEnumerable<Dtos.PatientImprovementGoalDto> Goals);

public class GetPatientImprovementGoalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patient-improvement-goals/{patientProfileId:guid}",
            async (Guid patientProfileId, [AsParameters] QueryDate query, ISender sender) =>
            {
                var queryObj = new GetPatientImprovementGoalQuery(patientProfileId, query.Date);
                var result = await sender.Send(queryObj);
                var response = result.Adapt<GetPatientImprovementGoalResponse>();
                return Results.Ok(response);
            })
            .WithName("GetPatientImprovementGoals")
            .WithTags("Patient Improvement Goals")
            .WithSummary("GetPatientImprovementGoals")
            .WithDescription("GetPatientImprovementGoals")
            .Produces<GetPatientImprovementGoalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public record QueryDate(DateTime Date);
}
