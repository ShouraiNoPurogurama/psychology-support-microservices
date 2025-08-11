using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.CreateServicePackage;

public record CreateServicePackageRequest(CreateServicePackageDto ServicePackage);
public record CreateServicePackageResponse(Guid Id);

public class CreateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("service-packages", async (
            [FromBody] CreateServicePackageRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            if(!AuthorizationHelpers.CanModifySystemData(httpContext.User))
                throw new ForbiddenException();
            
            var command = request.Adapt<CreateServicePackageCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<CreateServicePackageResponse>();

            return Results.Created($"/service-packages/{response.Id}", response);
        })
            
        .WithName("CreateServicePackage")
        .WithTags("ServicePackages")
        .Produces<CreateServicePackageResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create a new service package")
        .WithSummary("Create Service Package");
    }
}
