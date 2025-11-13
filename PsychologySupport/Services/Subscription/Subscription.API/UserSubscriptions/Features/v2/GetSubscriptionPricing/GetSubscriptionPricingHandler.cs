using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using MassTransit;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v2.GetSubscriptionPricing
{
    public record GetSubscriptionPricingCommand(GetSubscriptionPricingDto PricingDto)
        : ICommand<GetSubscriptionPricingResult>;

    public record GetSubscriptionPricingResult(GetSubscriptionPricingResponseDto Response);

    public class GetSubscriptionPricingHandler(
        SubscriptionDbContext context,
        IRequestClient<GetPatientProfileRequest> getPatientProfileClient,
        PromotionService.PromotionServiceClient promotionService)
        : ICommandHandler<GetSubscriptionPricingCommand, GetSubscriptionPricingResult>
    {
        public async Task<GetSubscriptionPricingResult> Handle(GetSubscriptionPricingCommand request, CancellationToken cancellationToken)
        {
            var dto = request.PricingDto;
            string status = "Áp dụng thành công"; // Mặc định là OK

            // Lấy ServicePackage để có giá gốc
            var servicePackage = await context.ServicePackages
                .FindAsync([dto.ServicePackageId], cancellationToken)
                ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

            var originalPrice = servicePackage.Price;
            var discountAmount = 0m;

            // Áp dụng Promo Code nếu có
            if (!string.IsNullOrEmpty(dto.PromoCode))
            {
                var promotionResponse = await promotionService.GetPromotionByCodeAsync(
                    new GetPromotionByCodeRequest
                    {
                        Code = dto.PromoCode,
                        IgnoreExpired = false
                    },
                    cancellationToken: cancellationToken
                );

                var promotion = promotionResponse.PromoCode;

                if (promotion is null)
                {
                    status = "Khuyến mãi không tồn tại hoặc đã hết hạn.";
                }
                else
                {
                    discountAmount += promotion.Value;
                }
            }

            // Áp dụng Gift nếu có
            if (dto.GiftId.HasValue)
            {
                var giftResponse = await promotionService.GetGiftCodeByPatientIdAsync(
                    new GetGiftCodeByPatientIdRequest
                    {
                        Id = dto.PatientId.ToString()
                    },
                    cancellationToken: cancellationToken
                );

                var giftCode = giftResponse.GiftCode
                    .FirstOrDefault(g => g.Id == dto.GiftId.ToString());

                if (giftCode is null)
                {
                    status = "Mã quà tặng không hợp lệ hoặc đã được sử dụng.";
                }
                else
                {
                    discountAmount += (decimal)giftCode.MoneyValue;
                }
            }

            var finalPrice = Math.Max(originalPrice - discountAmount, 0);

            var response = new GetSubscriptionPricingResponseDto(
                OriginalPrice: originalPrice,
                DiscountAmount: discountAmount,
                FinalPrice: finalPrice,
                Status: status
            );

            return new GetSubscriptionPricingResult(response);
        }
    }
}
