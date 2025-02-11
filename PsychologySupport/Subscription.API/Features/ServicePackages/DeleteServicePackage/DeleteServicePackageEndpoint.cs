using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Subscription.API.Features.ServicePackages.DeleteServicePackage;

public record DeleteServicePackageRequest(Guid Id);

public record DeleteServicePackageResponse(bool IsSuccess);

public class DeleteServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("service-packages", async ([FromBody] DeleteServicePackageRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteServicePackageCommand(request.Id));

            var response = result.Adapt<DeleteServicePackageResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteServicePackage")
        .Produces<DeleteServicePackageResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Service Package")
        .WithSummary("Delete Service Package");
    }
}