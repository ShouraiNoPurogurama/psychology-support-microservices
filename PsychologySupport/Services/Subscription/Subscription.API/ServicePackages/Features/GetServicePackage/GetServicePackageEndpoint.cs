using Carter;
using Mapster;
using MediatR;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.GetServicePackage;

public record GetServicePackageResponse(ServicePackageDto ServicePackage);

public class GetServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-packages/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetServicePackageQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetServicePackageResponse>();

                return Results.Ok(response);
            })
            .WithName("GetServicePackage")
            .Produces<GetServicePackageResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Service Package")
            .WithSummary("Get Service Package");
    }
}