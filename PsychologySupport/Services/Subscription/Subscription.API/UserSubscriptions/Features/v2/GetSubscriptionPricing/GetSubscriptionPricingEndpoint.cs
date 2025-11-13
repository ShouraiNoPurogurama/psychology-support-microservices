using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.GetSubscriptionPricing
{
    public record GetSubscriptionPricingRequest(
       Guid ServicePackageId,
       string? PromoCode,
       Guid? GiftId
   );

    public record GetSubscriptionPricingResponse(GetSubscriptionPricingResponseDto Response);

    public class GetSubscriptionPricingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/v2/user-subscription/pricing", async (
                [FromBody] GetSubscriptionPricingRequest request,
                ISender sender,
                HttpContext httpContext) =>
            {
                var patientId = httpContext.User.FindFirst("patientId")?.Value;
                if (string.IsNullOrEmpty(patientId))
                    throw new ForbiddenException("No patientId claim found in token.");
                if (!Guid.TryParse(patientId, out var patientGuid))
                    throw new ForbiddenException("Invalid patientId claim format.");

                var dto = request.Adapt<GetSubscriptionPricingDto>() with { PatientId = patientGuid };
                var command = new GetSubscriptionPricingCommand(dto);
                var result = await sender.Send(command);

                var response = result.Adapt<GetSubscriptionPricingResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("Get Subscription Pricing v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<GetSubscriptionPricingResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Tính toán giá subscription với promo code và gift (giá gốc, số tiền giảm, giá cuối, trạng thái áp dụng).")
            .WithSummary("Get original price, discount amount, final price, and apply status for subscription");
        }
    }
}
