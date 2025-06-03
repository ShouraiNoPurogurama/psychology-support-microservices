using Carter;
using MediatR;

namespace Scheduling.API.Features.GetTotalBooking
{
    public class GetTotalBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings/count", async ([AsParameters] GetTotalBookingQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return Results.Ok(result);
            })
            .WithName("GetTotalBookings")
            .WithTags("Bookings")             .Produces<GetTotalBookingResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetTotalBookings")
            .WithSummary("Get total bookings");
        }
    }
}
