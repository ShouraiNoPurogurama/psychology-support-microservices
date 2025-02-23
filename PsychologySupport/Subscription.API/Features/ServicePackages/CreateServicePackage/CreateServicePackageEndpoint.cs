using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Models;

namespace Subscription.API.Features.ServicePackages.CreateServicePackage;

public record CreateServicePackageRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId,
    bool IsActive
);


public record CreateServicePackageResponse(Guid Id);

public class CreateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("service-packages", async ([FromBody] CreateServicePackageRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateServicePackageCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateServicePackageResponse>();

            return Results.Created($"/service-packages/{response.Id}", response);
        }
        )
        .WithName("CreateServicePackage")
        .Produces<CreateServicePackageResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create Service Package")
        .WithSummary("Create Service Package");
    }
}