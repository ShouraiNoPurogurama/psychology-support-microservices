using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;

namespace Subscription.API.UserSubscriptions.Features.GetTotalServicePackage
{
    public class GetTotalServicePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/service-packages/total", async (
                [FromQuery] DateOnly startDate,
                [FromQuery] DateOnly endDate,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                var query = new GetTotalServicePackageQuery(startDate, endDate);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .WithName("GetTotalServicePackages")
            .WithTags("ServicePackages")
            .Produces<List<ServicePackageWithTotal>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all active service packages with total active subscriptions")
            .WithSummary("Get Service Packages with Active Subscription Count");
        }
    }
}
