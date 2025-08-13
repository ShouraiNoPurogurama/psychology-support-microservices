using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features02.CreateServicePackage;

public record CreateServicePackageV2Request(CreateServicePackageDto ServicePackage);
public record CreateServicePackageV2Response(Guid Id);

public class CreateServicePackageV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("v2/service-packages", async (
            [FromBody] CreateServicePackageV2Request request,
            ISender sender, HttpContext httpContext) =>
        {
            if(!AuthorizationHelpers.CanModifySystemData(httpContext.User))
                throw new ForbiddenException();
            
            var command = request.Adapt<CreateServicePackageCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<CreateServicePackageV2Response>();

            return Results.Created($"/v2/{response.Id}/servicePackages", response);
        })
            
        .WithName("CreateServicePackage v2")
        .WithTags("ServicePackages Version 2")
        .Produces<CreateServicePackageV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create a new service package")
        .WithSummary("Create Service Package");
    }
}
