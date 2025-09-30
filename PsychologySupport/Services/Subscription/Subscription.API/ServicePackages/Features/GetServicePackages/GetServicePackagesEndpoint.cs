using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.ServicePackages.Dtos;


namespace Subscription.API.ServicePackages.Features.GetServicePackages;

public record GetServicePackagesResponse(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/service-packages", async (
            [AsParameters] GetServicePackagesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);

            var response = result.Adapt<GetServicePackagesResponse>();

            return Results.Ok(response);
        })
        .WithName("GetServicePackages")
        .WithTags("ServicePackages")
        .Produces<GetServicePackagesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("GetServicePackages")
        .WithSummary("GetServicePackages");
    }
}