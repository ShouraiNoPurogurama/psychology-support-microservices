using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features02.GetServicePackage;

public record GetServicePackageV2Response(ServicePackageDto ServicePackage);

public class GetServicePackageV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/servicePackage/{id}", async ([FromRoute]Guid id, ISender sender) =>
            {
                var query = new GetServicePackageQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetServicePackageV2Response>();

                return Results.Ok(response);
            })
            .WithName("GetServicePackage v2")
            .WithTags("ServicePackages Version 2")
            .Produces<GetServicePackageV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Service Package")
            .WithSummary("Get Service Package");
    }
}