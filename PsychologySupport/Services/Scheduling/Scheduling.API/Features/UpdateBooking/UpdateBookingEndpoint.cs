using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.UpdateBooking
{
    public record UpdateBookingRequest(BookingStatus Status);

    public record UpdateBookingResponse(string BookingCode);

    public class UpdateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/bookings/{bookingcode}/status",
                    async ([FromRoute] string bookingcode, [FromBody] UpdateBookingRequest request, ISender sender) =>
                    {
                        var command = new UpdateBookingCommand(bookingcode, request.Status);
                        var result = await sender.Send(command);
                        var response = result.Adapt<UpdateBookingResponse>();
                        return Results.Ok(response);
                    })
                .WithName("Update Booking Status")
                .Produces<UpdateBookingResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Update Booking Status")
                .WithSummary("Update Booking Status");
        }
    }
}
