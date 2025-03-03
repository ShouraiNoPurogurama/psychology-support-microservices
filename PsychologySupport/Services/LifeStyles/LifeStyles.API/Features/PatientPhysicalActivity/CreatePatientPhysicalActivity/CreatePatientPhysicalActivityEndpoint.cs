using Carter;
using LifeStyles.API.Data.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity;

public record CreatePatientPhysicalActivityRequest(
    Guid PatientProfileId,
    List<PhysicalPreference> Activities
);

public record PhysicalPreference(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientPhysicalActivityResponse(
    Guid PatientProfileId,
    List<PhysicalPreference> Activities
);

public class CreatePatientPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patient-physical-activities", async (
                [FromBody] CreatePatientPhysicalActivityRequest request, ISender sender) =>
            {
                var command = new CreatePatientPhysicalActivityCommand(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.PhysicalActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = new CreatePatientPhysicalActivityResponse(
                    request.PatientProfileId,
                    request.Activities
                );

                return Results.Created($"/patient-physical-activities/{request.PatientProfileId}", response);
            })
            .WithName("CreatePatientPhysicalActivity")
            .Produces<CreatePatientPhysicalActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Physical Activities")
            .WithSummary("Create multiple Patient Physical Activities");
    }
}