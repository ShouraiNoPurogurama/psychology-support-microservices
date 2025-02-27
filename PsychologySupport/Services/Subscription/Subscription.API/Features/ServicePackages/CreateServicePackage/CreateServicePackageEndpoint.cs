using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Services;

namespace Subscription.API.Features.ServicePackages.CreateServicePackage;

public record CreateServicePackageRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    IFormFile ImageData
);

public record CreateServicePackageResponse(Guid Id);

public class CreateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("service-packages", async (
                [FromServices] IImageService imageService,
                [FromForm] CreateServicePackageRequest request,
                ISender sender) =>
            {
                var servicePackageId = Guid.NewGuid();

                Guid imageId = await imageService.UploadImageAsync(request.ImageData, "Service", servicePackageId);

                var command = new CreateServicePackageCommand(
                    servicePackageId,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.DurationDays,
                    imageId
                );

                var result = await sender.Send(command);

                var response = new CreateServicePackageResponse(result.Id);
                return Results.Created($"/service-packages/{response.Id}", response);
            })
            .WithName("CreateServicePackage")
            .Produces<CreateServicePackageResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new service package")
            .WithSummary("Create Service Package")
            .DisableAntiforgery();
    }
}
