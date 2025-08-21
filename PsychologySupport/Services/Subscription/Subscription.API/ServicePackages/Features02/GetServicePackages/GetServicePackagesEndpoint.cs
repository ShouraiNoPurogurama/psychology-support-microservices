using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Subscription.API.ServicePackages.Dtos;


namespace Subscription.API.ServicePackages.Features02.GetServicePackages;

public record GetServicePackagesV2Response(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/service-packages", async (
            [AsParameters] GetServicePackagesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);

            var response = result.Adapt<GetServicePackagesV2Response>();

            return Results.Ok(response);
        })
        .WithName("GetServicePackages v2")
        .WithTags("ServicePackages Version 2")
        .Produces<GetServicePackagesV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("GetServicePackages")
        .WithSummary("GetServicePackages");
    }
}