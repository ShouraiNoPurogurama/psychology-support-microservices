using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.GetBooking
{
    public record GetBookingResponse(BookingDto Booking);
    public class GetBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings/{bookingCode}", async (string bookingCode, ISender sender) =>
            {
                var query = new GetBookingQuery(bookingCode);
                var result = await sender.Send(query);
                var response = result.Adapt<GetBookingResponse>();
                return Results.Ok(response);
            })
                .WithName("GetBooking")
                .WithTags("Bookings")
                .Produces<GetBookingResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Get Booking")
                .WithSummary("Get Booking");
        }
    }
}
