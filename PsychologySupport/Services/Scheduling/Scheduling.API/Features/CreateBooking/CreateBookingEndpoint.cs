using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Scheduling.API.Features.CreateBooking
{
    public record CreateBookingResponse(Guid BookingId, string BookingCode, string PaymentUrl);

    public class CreateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings", async ([FromBody] CreateBookingCommand request, ISender sender) =>
                {
                    var result = await sender.Send(request);

                    var response = result.Adapt<CreateBookingResponse>();

                    return Results.Ok(response);
                })
                .WithName("CreateBooking")
                .Produces<CreateBookingResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Create Booking")
                .WithSummary("Create Booking");
        }
    }
}