using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.CreateUserSubscription;

public record CreateUserSubscriptionV2Request(
    Guid ServicePackageId,
    string? PromoCode,
    Guid? GiftId,
    DateTimeOffset StartDate,
    PaymentMethodName PaymentMethodName
);

public record CreateUserSubscriptionV2Response(Guid Id, string PaymentUrl, long? PaymentCode);

public class CreateUserSubscriptionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/user-subscription", async (
         [FromBody] CreateUserSubscriptionV2Request request,
         ISender sender,
         HttpContext httpContext) =>
        {
            // Lấy patientId từ claim trong token
            var patientId = httpContext.User.FindFirst("patientId")?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new ForbiddenException("No patientId claim found in token.");

            if (!Guid.TryParse(patientId, out var patientGuid))
                throw new ForbiddenException("Invalid patientId claim format.");


            var dto = request.Adapt<CreateUserSubscriptionDto>();

           
            dto = dto with { PatientId = patientGuid };

   
            var command = new CreateUserSubscriptionCommand(dto);

            var result = await sender.Send(command);


            var response = result.Adapt<CreateUserSubscriptionV2Response>();

            return Results.Created($"/v2/{response.Id}/users-subscription", response);
        })
         .WithName("CreateUserSubscription v2")
         .WithTags("UserSubscriptions Version 2")
         .Produces<CreateUserSubscriptionV2Response>()
         .ProducesProblem(StatusCodes.Status400BadRequest)
         .WithDescription("Create User Subscription")
         .WithSummary("Create User Subscription");

    }
}
