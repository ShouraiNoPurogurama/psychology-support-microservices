using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Common;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints02;

public record CreatePayOSCallBackUrlForUpgradeSubscriptionV2Request(UpgradeSubscriptionDto UpgradeSubscription);
public record CreatePayOSCallBackUrlForUpgradeSubscriptionV2Response(string Url);

public class CreatePayOSCallBackUrlForUpgradeSubscriptionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/payments/payos/upgrade-subscription", async (
            [FromBody] CreatePayOSCallBackUrlForUpgradeSubscriptionV2Request request,
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
            var response = result.Adapt<CreatePayOSCallBackUrlForUpgradeSubscriptionV2Response>();
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("CreatePayOSCallBackUrlForUpgradeSubscription v2")
        .WithTags("PayOS Payments Version 2")
        .Produces<CreatePayOSCallBackUrlForUpgradeSubscriptionV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Upgrade Subscription")
        .WithSummary("Create PayOS CallBack Url For Upgrade Subscription");
    }
}
