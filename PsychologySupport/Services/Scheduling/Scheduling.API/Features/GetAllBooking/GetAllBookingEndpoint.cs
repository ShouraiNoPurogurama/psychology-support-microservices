
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.GetAllBooking
{
    public record GetAllBookingsResponse(PaginatedResult<BookingDto> Bookings);
    public class GetAllBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", async ([AsParameters] GetAllBookingsQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = result.Adapt<GetAllBookingsResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
            .WithName("GetAllBookings")
            .WithTags("Bookings")
            .Produces<GetAllBookingsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Bookings")
            .WithSummary("Get All Bookings");
        }
    }


}
