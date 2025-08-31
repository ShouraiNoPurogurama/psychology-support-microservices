using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Subscription.API.UserSubscriptions.Features.v1.ActivateSubscription
{
    public record ActivateSubscriptionByGiftCodeRequest(Guid PatientId, string GiftCode);

    public record ActivateSubscriptionByGiftCodeResponse(Guid SubscriptionId);

    public class ActivateSubscriptionByGiftCodeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("user-subscriptions/activate-by-giftcode", async (
                    [FromBody] ActivateSubscriptionByGiftCodeRequest request,
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

                var response = result.Adapt<ActivateSubscriptionByGiftCodeResponse>();

                return Results.Ok(response);
            })
                .WithName("ActivateSubscriptionByGiftCode")
                .WithTags("UserSubscriptions")
                .Produces<ActivateSubscriptionByGiftCodeResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .WithDescription("Activate User Subscription by GiftCode")
                .WithSummary("Activate User Subscription by GiftCode");
        }
    }
}
