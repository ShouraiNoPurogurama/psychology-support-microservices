using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Profile.API.DoctorProfiles.Features.CreateDoctorProfile;
using Subscription.API.ServicePackages.Dtos;
using Subscription.API.Services;

namespace Subscription.API.ServicePackages.Features.CreateServicePackage;


public record CreateServicePackageRequest(CreateServicePackageDto ServicePackage);

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
            })
            .WithName("CreateServicePackage")
            .Produces<CreateServicePackageResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new service package")
            .WithSummary("Create Service Package");
    }
}
