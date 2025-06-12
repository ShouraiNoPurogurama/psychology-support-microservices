using BuildingBlocks.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientFoodActivity.CreatePatientFoodActivity;

public record CreatePatientFoodActivityRequest(
    Guid PatientProfileId,
    List<FoodPreference> Activities
);

public record FoodPreference(Guid FoodActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientFoodActivityResponse(
    Guid PatientProfileId,
    List<FoodPreference> Activities
);

public class CreatePatientFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("patient-Food-activities", async (
                [FromBody] CreatePatientFoodActivityRequest request, ISender sender) =>
            {
                var command = new CreatePatientFoodActivityCommand(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.FoodActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = new CreatePatientFoodActivityResponse(
                    request.PatientProfileId,
                    request.Activities
                );

                return Results.Created($"/patient-Food-activities/{request.PatientProfileId}", response);
            })
            .WithName("CreatePatientFoodActivity")
            .WithTags("PatientFoodActivities")
            .Produces<CreatePatientFoodActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Food Activities")
            .WithSummary("Create multiple Patient Food Activities");
    }
}