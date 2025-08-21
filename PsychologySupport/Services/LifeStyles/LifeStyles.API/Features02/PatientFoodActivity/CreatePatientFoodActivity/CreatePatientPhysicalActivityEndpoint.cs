using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features02.PatientFoodActivity.CreatePatientFoodActivity;

public record CreatePatientFoodActivityV2Request(
    Guid PatientProfileId,
    List<FoodPreferenceV2> Activities
);

public record FoodPreferenceV2(Guid FoodActivityId, PreferenceLevel PreferenceLevel);

public record CreatePatientFoodActivityV2Response(
    Guid PatientProfileId,
    List<FoodPreferenceV2> Activities
);

public class CreatePatientFoodActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/me/activities/food", async (HttpContext httpContext,[FromBody] CreatePatientFoodActivityV2Request request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();

                var command = new CreatePatientFoodActivityV2Command(
                    request.PatientProfileId,
                    request.Activities.Select(a => (a.FoodActivityId, a.PreferenceLevel)).ToList()
                );

                await sender.Send(command);

                var response = new CreatePatientFoodActivityV2Response(
                    request.PatientProfileId,
                    request.Activities
                );

                return Results.Created($"/v2/me/{request.PatientProfileId}/foodActivities", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientFoodActivity v2")
            .WithTags("PatientFoodActivities Version 2")
            .Produces<CreatePatientFoodActivityV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Patient Food Activities")
            .WithSummary("Create Patient Food Activities");
    }
}