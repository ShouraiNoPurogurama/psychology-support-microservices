using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Features.UpdateServicePackage;

public record UpdateServicePackageRequest(Guid Id, UpdateServicePackageDto ServicePackage);
public record UpdateServicePackageResponse(bool IsSuccess);

public class UpdateServicePackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-packages/{id}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateServicePackageDto dto,
            ISender sender, HttpContext httpContext) =>
        {
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to access this resource."
                );
            
            var command = new UpdateServicePackageCommand(id, dto);
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateServicePackageResponse>();

            return Results.Ok(response);
        })
        .WithName("UpdateServicePackage")
        .WithTags("Dashboard")
        .Produces<UpdateServicePackageResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Update Service Package")
        .WithSummary("Update Service Package");
    }
}
