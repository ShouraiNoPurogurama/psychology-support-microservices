using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints
{
    public class GetRevenueEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/payments/revenue", async (
                [FromQuery] DateOnly startTime,
                [FromQuery] DateOnly endTime,
                ISender sender,
                CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetRevenueQuery(startTime, endTime);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager", "User"))
            .WithName("GetRevenue")
            .WithTags("Dashboard")
            .Produces<GetRevenueResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Revenue")
            .WithDescription("Get Revenue");
        }
    }
}
