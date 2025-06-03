using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
                CancellationToken cancellationToken) =>
            {
                var query = new GetDailyRevenueQuery(startTime, endTime);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetDailyRevenue")
            .WithTags("Payments")
            .Produces<GetDailyRevenueResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Daily Revenue")
            .WithDescription("Get Daily Revenue");
        }
    }
}


