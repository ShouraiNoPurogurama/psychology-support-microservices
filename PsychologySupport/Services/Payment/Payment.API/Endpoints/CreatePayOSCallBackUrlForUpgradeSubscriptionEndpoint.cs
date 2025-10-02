using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreatePayOSCallBackUrlForUpgradeSubscriptionRequest(UpgradeSubscriptionDto UpgradeSubscription);
public record CreatePayOSCallBackUrlForUpgradeSubscriptionResponse(string Url);

public class CreatePayOSCallBackUrlForUpgradeSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/payments/payos/upgrade-subscription", async (
            [FromBody] CreatePayOSCallBackUrlForUpgradeSubscriptionRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.CanModifyPatientProfile(request.UpgradeSubscription.PatientId, httpContext.User))
                return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

            var command = new CreatePayOSCallBackUrlForUpgradeSubscriptionCommand(request.UpgradeSubscription);
            var result = await sender.Send(command);
            var response = result.Adapt<CreatePayOSCallBackUrlForUpgradeSubscriptionResponse>();
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("CreatePayOSCallBackUrlForUpgradeSubscription")
        .WithTags("PayOS Payments")
        .Produces<CreatePayOSCallBackUrlForUpgradeSubscriptionResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Upgrade Subscription")
        .WithSummary("Create PayOS CallBack Url For Upgrade Subscription");
    }
}
