using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Models;

namespace Subscription.API.Features.ServicePackages.GetServicePackages;

public record GetServicePackagesRequest(int PageNumber, int PageSize);

public record GetServicePackagesResponse(IEnumerable<ServicePackage> ServicePackages, int TotalCount);

public class GetServicePackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-packages", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetServicePackagesQuery(pageNumber, pageSize);
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
