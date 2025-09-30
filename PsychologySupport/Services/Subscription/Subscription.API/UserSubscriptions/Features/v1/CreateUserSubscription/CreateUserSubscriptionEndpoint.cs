using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v1.CreateUserSubscription;

public record CreateUserSubscriptionRequest(CreateUserSubscriptionDto UserSubscription);

public record CreateUserSubscriptionResponse(Guid Id, string PaymentUrl,long? PaymentCode);

public class CreateUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/user-subscription", async ([FromBody] CreateUserSubscriptionRequest request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.UserSubscription.PatientId, httpContext.User))
                        throw new ForbiddenException();

                    var command = request.Adapt<v2.CreateUserSubscription.CreateUserSubscriptionCommand>();

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