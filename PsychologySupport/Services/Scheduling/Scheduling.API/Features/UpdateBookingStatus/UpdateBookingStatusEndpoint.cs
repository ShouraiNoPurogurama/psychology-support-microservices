using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.UpdateBookingStatus;

public record UpdateBookingStatusRequest(Guid BookingId, BookingStatus Status);

public record UpdateBookingStatusResponse(bool IsSuccess);


public class UpdateBookingStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings/status", async ([FromBody] UpdateBookingStatusRequest request, ISender sender) =>
            {
                var command = new UpdateBookingStatusCommand(request.BookingId, request.Status);

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateBookingStatusResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateBookingStatus")
            .WithTags("Bookings")
            .Produces<UpdateBookingStatusResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Update Booking Status")
            .WithSummary("Update Booking Status");
    }
}