using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.UpdateServicePackage;

public record UpdateServicePackageRequest(ServicePackageDto ServicePackage);

public record UpdateServicePackageResponse(bool IsSuccess);

public class UpdateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-packages", async ([FromBody] UpdateServicePackageRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateServicePackageCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateServicePackageResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateServicePackage")
            .Produces<UpdateServicePackageResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Service Package")
            .WithSummary("Update Service Package");
    }
}