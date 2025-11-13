using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using MassTransit;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;

namespace Subscription.API.UserSubscriptions.Features.v2.GetSubscriptionPricing
{
    public record GetSubscriptionPricingCommand(GetSubscriptionPricingDto PricingDto) : ICommand<GetSubscriptionPricingResult>;

    public record GetSubscriptionPricingResult(decimal OriginalPrice, decimal DiscountAmount, decimal FinalPrice);

    public record GetSubscriptionPricingDto(
        Guid PatientId,
        Guid ServicePackageId,
        string? PromoCode,
        Guid? GiftId
    );

    public class GetSubscriptionPricingHandler(
        SubscriptionDbContext context,
        IRequestClient<GetPatientProfileRequest> getPatientProfileClient,
        PromotionService.PromotionServiceClient promotionService)
        : ICommandHandler<GetSubscriptionPricingCommand, GetSubscriptionPricingResult>
    {
        public async Task<GetSubscriptionPricingResult> Handle(GetSubscriptionPricingCommand request, CancellationToken cancellationToken)
        {
            var dto = request.PricingDto;

            // Lấy ServicePackage để có giá gốc
            ServicePackage servicePackage = await context.ServicePackages
                .FindAsync([dto.ServicePackageId], cancellationToken)
                ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

            var originalPrice = servicePackage.Price;
            var discountAmount = 0m;
            var finalPrice = originalPrice;

            // Áp dụng Promo Code nếu có
            if (!string.IsNullOrEmpty(dto.PromoCode))
            {
                var promotion = (await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest()
                {
                    Code = dto.PromoCode,
                    IgnoreExpired = false
                }, cancellationToken: cancellationToken)).PromoCode;

                if (promotion is not null)
                {
                    discountAmount += promotion.Value;
                }
            }

            // Áp dụng Gift nếu có
            if (dto.GiftId.HasValue)
            {
                var giftCode = (await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
                {
                    Id = dto.PatientId.ToString()
                }, cancellationToken: cancellationToken))
                .GiftCode
                .FirstOrDefault(g => g.Id == dto.GiftId.ToString());

                if (giftCode is not null)
                {
                    discountAmount += (decimal)giftCode.MoneyValue;
                }
            }

            finalPrice = Math.Max(originalPrice - discountAmount, 0);

            return new GetSubscriptionPricingResult(originalPrice, discountAmount, finalPrice);
        }
    }
}
