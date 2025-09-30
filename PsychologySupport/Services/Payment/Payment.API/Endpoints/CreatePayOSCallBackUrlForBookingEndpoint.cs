using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreatePayOSCallBackUrlForBookingRequest(BuyBookingDto BuyBooking);
public record CreatePayOSCallBackUrlForBookingResponse(string Url);

public class CreatePayOSCallBackUrlForBookingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/payments/payos/booking", async (
            [FromBody] CreatePayOSCallBackUrlForBookingRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.CanModifyPatientProfile(request.BuyBooking.PatientId, httpContext.User))
                return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

            var command = new CreatePayOSCallBackUrlForBookingCommand(request.BuyBooking);
            var result = await sender.Send(command);
            var response = result.Adapt<CreatePayOSCallBackUrlForBookingResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("CreatePayOSCallBackUrlForBooking")
        .WithTags("PayOS Payments")
        .Produces<CreatePayOSCallBackUrlForBookingResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Booking")
        .WithSummary("Create PayOS CallBack Url For Booking");
    }
}
