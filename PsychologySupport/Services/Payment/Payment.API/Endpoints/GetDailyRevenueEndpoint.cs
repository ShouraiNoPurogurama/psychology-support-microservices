﻿using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints
{
    public class GetDailyRevenueEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/payments/daily-revenue", async (
                [FromQuery] DateOnly startTime,
                [FromQuery] DateOnly endTime,
                ISender sender,
                CancellationToken cancellationToken, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetDailyRevenueQuery(startTime, endTime);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin", "User"))
            .WithName("GetDailyRevenue")
            .WithTags("Dashboard")
            .Produces<GetDailyRevenueResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Daily Revenue")
            .WithDescription("Get Daily Revenue");
        }
    }
}


