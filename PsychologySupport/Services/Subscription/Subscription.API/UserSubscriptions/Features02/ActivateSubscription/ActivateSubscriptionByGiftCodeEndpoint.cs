using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;

namespace Subscription.API.UserSubscriptions.Features02.ActivateSubscription
{
    public record ActivateSubscriptionByGiftCodeV2Request(Guid PatientId, string GiftCode);

    public record ActivateSubscriptionByGiftCodeV2Response(Guid SubscriptionId);

    public class ActivateSubscriptionByGiftCodeV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/v2/user-subscriptions/activate-by-giftcode", async (
                    [FromBody] ActivateSubscriptionByGiftCodeV2Request request,
                    ISender sender,
                    HttpContext httpContext) =>
            {
                //// Authorization check
                //if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                //    return Results.Problem(
                //        statusCode: StatusCodes.Status403Forbidden,
                //        title: "Forbidden",
                //        detail: "You do not have permission to activate this subscription."
                //    );

                var command = request.Adapt<ActivateSubscriptionByGiftCodeCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<ActivateSubscriptionByGiftCodeV2Response>();

                return Results.Ok(response);
            })
                .WithName("ActivateSubscriptionByGiftCode v2")
                .WithTags("UserSubscriptions Version 2")
                .Produces<ActivateSubscriptionByGiftCodeV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .WithDescription("Activate User Subscription by GiftCode")
                .WithSummary("Activate User Subscription by GiftCode");
        }
    }
}
