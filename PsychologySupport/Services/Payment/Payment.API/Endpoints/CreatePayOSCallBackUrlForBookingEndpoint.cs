using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreatePayOSCallBackUrlForBookingRequest(BuyBookingDto BuyBooking);
public record CreatePayOSCallBackUrlForBookingResponse(string Url);

public class CreatePayOSCallBackUrlForBookingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/payos/booking", async (
            [FromBody] CreatePayOSCallBackUrlForBookingRequest request,
            ISender sender) =>
        {
            var command = new CreatePayOSCallBackUrlForBookingCommand(request.BuyBooking);
            var result = await sender.Send(command);
            var response = result.Adapt<CreatePayOSCallBackUrlForBookingResponse>();
            return Results.Ok(response);
        })
        .WithName("CreatePayOSCallBackUrlForBooking")
        .WithTags("PayOS Payments")
        .Produces<CreatePayOSCallBackUrlForBookingResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Booking")
        .WithSummary("Create PayOS CallBack Url For Booking");
    }
}
