using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features02.CreateUserSubscription;

public record CreateUserSubscriptionV2Request(CreateUserSubscriptionDto UserSubscription);

public record CreateUserSubscriptionV2Response(Guid Id, string PaymentUrl,long? PaymentCode);

public class CreateUserSubscriptionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/userSubscription", async ([FromBody] CreateUserSubscriptionV2Request request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.UserSubscription.PatientId, httpContext.User))
                        throw new ForbiddenException();

                    var command = request.Adapt<CreateUserSubscriptionCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateUserSubscriptionV2Response>();

                    return Results.Created($"/v2/{response.Id}/userSubscriptions", response);
                }
            )
            .WithName("CreateUserSubscription v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<CreateUserSubscriptionV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create User Subscription")
            .WithSummary("Create User Subscription");
    }
}