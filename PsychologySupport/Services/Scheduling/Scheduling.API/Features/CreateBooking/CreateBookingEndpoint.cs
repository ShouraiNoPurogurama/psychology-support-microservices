using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Scheduling.API.Features.CreateBooking
{
    public record CreateBookingResponse(Guid BookingId, string BookingCode);

    public class CreateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("bookings",
                async ([FromBody] CreateBookingCommand request, ISender sender) =>
                {
                    var command = new CreateBookingCommand(
                        request.DoctorId,
                        request.PatientId,
                        request.Date,
                        request.StartTime,
                        request.Duration,
                        request.Price,
                        request.PromoCodeId,
                        request.GiftCodeId
                    );

                    var result = await sender.Send(command);

                    var response = new CreateBookingResponse(result.BookingId, result.BookingCode);

                    return Results.Created($"/bookings/{result.BookingId}", response);
                })
                .WithName("CreateBooking")
                .Produces<CreateBookingResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Create Booking")
                .WithSummary("Create Booking");
        }
    }
}
