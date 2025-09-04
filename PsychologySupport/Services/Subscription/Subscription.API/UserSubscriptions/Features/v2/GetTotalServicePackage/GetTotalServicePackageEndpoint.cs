using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;

namespace Subscription.API.UserSubscriptions.Features.v2.GetTotalServicePackage
{
    public record GetTotalServicePackageV2Request(DateOnly StartDate, DateOnly EndDate);

    public record ServicePackageWithTotalDto(Guid Id, string Name, long TotalSubscriptions);

    public record GetTotalServicePackageV2Response(List<ServicePackageWithTotalDto> ServicePackages);
    public class GetTotalServicePackageV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/service-packages/total", async (
                    [FromBody] GetTotalServicePackageV2Request request,
                    ISender sender,
                    HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) &&
                    !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();

                var query = request.Adapt<GetTotalServicePackageQuery>();

                var result = await sender.Send(query);

                var response = result.Adapt<GetTotalServicePackageV2Response>();

                return Results.Ok(response);
            })
                .WithName("GetTotalServicePackagesV2")
                .WithTags("Dashboard Version 2")
                .Produces<GetTotalServicePackageV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get all active service packages with total active subscriptions")
                .WithSummary("Get Service Packages with Active Subscription Count");
        }
    }
}
