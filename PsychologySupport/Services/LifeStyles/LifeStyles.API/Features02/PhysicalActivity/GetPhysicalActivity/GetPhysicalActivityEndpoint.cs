using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.PhysicalActivity.GetPhysicalActivity;

public record GetPhysicalActivityV2Request(Guid Id);

public record GetPhysicalActivityV2Response(PhysicalActivityDto PhysicalActivity);

public class GetPhysicalActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/physicalActivities/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetPhysicalActivityV2Query(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetPhysicalActivityV2Response>();

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithTags("PhysicalActivities Version 2")
            .WithName("GetPhysicalActivity v2")
            .Produces<GetPhysicalActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Physical Activity")
            .WithSummary("Get Physical Activity");
    }
}