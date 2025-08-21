using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features02.UpdateServicePackage;

public record UpdateServicePackageV2Request(Guid Id, UpdateServicePackageDto ServicePackage);
public record UpdateServicePackageV2Response(bool IsSuccess);

public class UpdateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v2/service-package/{id}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateServicePackageDto dto,
            ISender sender, HttpContext httpContext) =>
        {
            if (!(AuthorizationHelpers.CanModifySystemData(httpContext.User) || AuthorizationHelpers.IsExclusiveAccess(httpContext.User)))
                throw new ForbiddenException();

            var command = new UpdateServicePackageCommand(id, dto);
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateServicePackageV2Response>();

            return Results.Ok(response);
        })
        .WithName("UpdateServicePackage v2")
        .WithTags("ServicePackages Version 2")
        .Produces<UpdateServicePackageV2Response>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Update Service Package")
        .WithSummary("Update Service Package");
    }
}
