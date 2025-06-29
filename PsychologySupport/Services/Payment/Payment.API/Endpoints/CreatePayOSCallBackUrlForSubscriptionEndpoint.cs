using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreatePayOSCallBackUrlForSubscriptionRequest(BuySubscriptionDto BuySubscription);
public record CreatePayOSCallBackUrlForSubscriptionResponse(string Url);

public class CreatePayOSCallBackUrlForSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/payos/subscription", async (
            [FromBody] CreatePayOSCallBackUrlForSubscriptionRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.CanModifyPatientProfile(request.BuySubscription.PatientId, httpContext.User))
                return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

            var command = new CreatePayOSCallBackUrlForSubscriptionCommand(request.BuySubscription);
            var result = await sender.Send(command);
            var response = result.Adapt<CreatePayOSCallBackUrlForSubscriptionResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("CreatePayOSCallBackUrlForSubscription")
        .WithTags("PayOS Payments")
        .Produces<CreatePayOSCallBackUrlForSubscriptionResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Subscription")
        .WithSummary("Create PayOS CallBack Url For Subscription");
    }
}
