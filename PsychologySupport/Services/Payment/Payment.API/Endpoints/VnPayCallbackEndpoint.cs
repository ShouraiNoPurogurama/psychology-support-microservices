using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Dtos;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints;

public class VnPayCallbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/callback", async ([AsParameters] VnPayCallbackDto vnPayCallbackRequest, ISender sender) =>
        {
            var query = new VnPayCallbackQuery(vnPayCallbackRequest);

            var result = await sender.Send(query);

            return Results.Ok(result);
        });
    }
}