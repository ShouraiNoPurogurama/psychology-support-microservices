using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features02.EntertainmentActivity.GetEntertainmentActivity;

public record GetEntertainmentActivityV2Request(Guid Id);

public record GetEntertainmentActivityV2Response(EntertainmentActivityDto EntertainmentActivity);

public class GetEntertainmentActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/entertainmentActivities/{id:guid}", async (
                Guid id,
                ISender sender,
                HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to access this resource."
                );

            var query = new GetEntertainmentActivityV2Query(id);
            var result = await sender.Send(query);

            var response = result.Adapt<GetEntertainmentActivityV2Response>();
            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithTags("EntertainmentActivities Version 2")
            .Produces<GetEntertainmentActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Entertainment Activity")
            .WithSummary("Get Entertainment Activity");
    }
}