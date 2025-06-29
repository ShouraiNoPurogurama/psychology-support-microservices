using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.CreateUserSubscription;

public record CreateUserSubscriptionRequest(CreateUserSubscriptionDto UserSubscription);

public record CreateUserSubscriptionResponse(Guid Id, string PaymentUrl,long? PaymentCode);

public class CreateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("user-subscriptions", async ([FromBody] CreateUserSubscriptionRequest request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.UserSubscription.PatientId, httpContext.User))
                        return Results.Problem(
                                       statusCode: StatusCodes.Status403Forbidden,
                                       title: "Forbidden",
                                       detail: "You do not have permission to access this resource."
                                   );

                    var command = request.Adapt<CreateUserSubscriptionCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateUserSubscriptionResponse>();

                    return Results.Created($"/user-subscriptions/{response.Id}", response);
                }
            )
            .WithName("CreateUserSubscription")
            .WithTags("UserSubscriptions")
            .Produces<CreateUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create User Subscription")
            .WithSummary("Create User Subscription");
    }
}