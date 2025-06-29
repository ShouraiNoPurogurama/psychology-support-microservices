using BuildingBlocks.Enums;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Common;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.CreateBooking
{
    public record CreateBookingRequest(
        Guid DoctorId,
        Guid PatientId,
        DateOnly Date,
        TimeOnly StartTime,
        int Duration,
        decimal Price,
        string? PromoCode,
        Guid? GiftCodeId,
        PaymentMethodName PaymentMethod
    );
    public record CreateBookingResponse(Guid BookingId, string BookingCode, string PaymentUrl);

    public class CreateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings", async (
            [FromBody] CreateBookingRequest request,
            ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                    return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );

                var dto = request.Adapt<CreateBookingDto>();
                var result = await sender.Send(new CreateBookingCommand(dto));

                var response = result.Adapt<CreateBookingResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateBooking")
            .WithTags("Bookings")
            .Produces<CreateBookingResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Booking")
            .WithSummary("Create Booking");
        }
    }
}
