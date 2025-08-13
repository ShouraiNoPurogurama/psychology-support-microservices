using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features02.PatientFoodActivity.GetPatientFoodActivity;

public record GetPatientFoodActivityV2Request(Guid PatientProfileId);

public record GetPatientFoodActivityV2Response(IEnumerable<Models.PatientFoodActivity> PatientFoodActivities);

public class GetPatientFoodActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/activities/foods",
                async (HttpContext httpContext,Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        throw new ForbiddenException();

                    var query = new GetPatientFoodActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientFoodActivityV2Response>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientFoodActivities v2")
            .WithTags("PatientFoodActivities Version 2")
            .Produces<GetPatientFoodActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Food Activities")
            .WithSummary("Get Patient Food Activities");
    }
}