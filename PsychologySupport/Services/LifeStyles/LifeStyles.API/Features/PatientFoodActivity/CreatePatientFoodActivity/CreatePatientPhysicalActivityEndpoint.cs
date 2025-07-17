using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        app.MapPost("patient-Food-activities", async (HttpContext httpContext,[FromBody] CreatePatientFoodActivityRequest request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

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
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientFoodActivity")
            .WithTags("PatientFoodActivities")
            .Produces<CreatePatientFoodActivityResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create multiple Patient Food Activities")
            .WithSummary("Create multiple Patient Food Activities");
    }
}