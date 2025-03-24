using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.GetServicePackages;

public record GetServicePackagesResponse(PaginatedResult<ServicePackageDto> ServicePackages);

public class GetServicePackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-packages",
                async ([FromQuery] Guid? patientId, [AsParameters] PaginationRequest request, ISender sender) =>
                {
                    var query = new GetServicePackagesQuery(request, patientId);
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