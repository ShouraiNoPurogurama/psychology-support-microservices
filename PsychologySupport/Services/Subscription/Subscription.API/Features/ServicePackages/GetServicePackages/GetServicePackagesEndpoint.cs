using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.Dtos;

namespace Subscription.API.Features.ServicePackages.GetServicePackages;

public record GetServicePackagesResponse(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-packages", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetServicePackagesQuery(request);
                var result = await sender.Send(query);

                var response = result.Adapt<GetServicePackagesResponse>();

                return Results.Ok(response);
            })
            .WithName("GetServicePackages")
            .Produces<GetServicePackagesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Service Packages")
            .WithSummary("Get Service Packages");
    }
}