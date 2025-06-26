using Carter;
using MediatR;

namespace Scheduling.API.Features.GetTop5Booking
{
    public class GetTop5BookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings/top-doctors", async ([AsParameters] GetTopDoctorsQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin"))
            .WithName("GetTop5DoctorsByBooking")
            .WithTags("Bookings")
            .Produces<GetTopDoctorsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Top 5 Doctors By Booking")
            .WithSummary("Get Top 5 Doctors by Booking");
        }
    }
}
