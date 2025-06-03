using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreateVnPayCallBackUrlForBookingRequest(BuyBookingDto BuyBooking);

public record CreateVnPayCallBackUrlForBookingResponse(string Url);

public class CreateVnPayCallBackUrlForBookingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/vnpay/booking", async ([FromBody]CreateVnPayCallBackUrlForBookingRequest request, ISender sender) =>
            {
                var command = new CreateVnPayCallBackUrlForBookingCommand(request.BuyBooking);
                
                var result = await sender.Send(command);

                var response = result.Adapt<CreateVnPayCallBackUrlForBookingResponse>();
                
                return Results.Ok(response);
            })
            .WithName("CreateVnPayCallBackUrlForBooking")
            .WithTags("Booking Payments")
            .Produces<CreateVnPayCallBackUrlForBookingResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create VnPay CallBack Url For Booking")
            .WithSummary("Create VnPay CallBack Url For Booking");
    }
}