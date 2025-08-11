using Carter;
using LifeStyles.API.Dtos;
using LifeStyles.API.Features.TherapeuticActivity.GetTherapeuticActivity;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.TherapeuticActivity.GetTherapeuticActivity;

public record GetTherapeuticActivityV2Request(Guid Id);

public record GetTherapeuticActivityV2Response(TherapeuticActivityDto TherapeuticActivity);

public class GetTherapeuticActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/therapeuticActivities/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetTherapeuticActivityV2Query(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetTherapeuticActivityV2Response>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithTags("TherapeuticActivities Version 2")
            .WithName("GetTherapeuticActivity v2")
            .Produces<GetTherapeuticActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Therapeutic Activity")
            .WithSummary("Get Therapeutic Activity");
    }
}