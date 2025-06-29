using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.EntertainmentActivity.GetEntertainmentActivity;

public record GetEntertainmentActivityRequest(Guid Id);

public record GetEntertainmentActivityResponse(EntertainmentActivityDto EntertainmentActivity);

public class GetEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/entertainment-activities/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );
                var query = new GetEntertainmentActivityQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetEntertainmentActivityResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetEntertainmentActivity")
            .Produces<GetEntertainmentActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Entertainment Activity")
            .WithSummary("Get Entertainment Activity");
    }
}