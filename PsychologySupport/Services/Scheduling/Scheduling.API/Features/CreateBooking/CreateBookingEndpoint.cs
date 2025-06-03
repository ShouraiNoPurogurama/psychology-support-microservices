using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.CreateBooking
{
    public record CreateBookingResponse(Guid BookingId, string BookingCode, string PaymentUrl);

    public class CreateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings", async (
                [FromBody] CreateBookingCommand request,
                ISender sender) =>
            {
               /* var validationResult = await validator.ValidateAsync(request.Adapt<CreateBookingDto>());
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
*/
                var result = await sender.Send(request);
                var response = result.Adapt<CreateBookingResponse>();

                return Results.Ok(response);
            })
            .WithName("CreateBooking")
            .WithTags("Bookings")
            .Produces<CreateBookingResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Booking")
            .WithSummary("Create Booking");
        }
    }
}
