using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.v2.ActivateSubscription
{
    public record ActivateSubscriptionByGiftCodeCommand(Guid PatientId, string GiftCode) : IRequest<ActivateSubscriptionByGiftCodeResult>;

    public record ActivateSubscriptionByGiftCodeResult(Guid SubscriptionId);
    public class ActivateSubscriptionByGiftCodeHandler(
    SubscriptionDbContext context,
    PromotionService.PromotionServiceClient promotionService
) : IRequestHandler<ActivateSubscriptionByGiftCodeCommand, ActivateSubscriptionByGiftCodeResult>
    {
        public async Task<ActivateSubscriptionByGiftCodeResult> Handle(ActivateSubscriptionByGiftCodeCommand request, CancellationToken cancellationToken)
        {
            // Validate gift code via gRPC
            var giftCodeResponse = await promotionService.GetGiftCodeByCodeAsync(
                new GetGiftCodeByCodeRequest { Code = request.GiftCode }, cancellationToken: cancellationToken);

            var giftCode = giftCodeResponse.GiftCode;
            if (giftCode == null || !giftCode.IsActive)
                throw new Exception("Gift code is invalid or inactive.");


            // Check for existing active subscription
            var hasActive = await context.UserSubscriptions
                .AnyAsync(x => x.PatientId == request.PatientId && x.Status == SubscriptionStatus.Active, cancellationToken);
            if (hasActive)
                throw new Exception("Patient already has an active subscription.");

            // Create subscription
            var startDate = DateTime.UtcNow;
            var servicePackageId = Guid.Parse(giftCode.ServicePackageId);

            var servicePackage = await context.ServicePackages
                .FirstOrDefaultAsync(sp => sp.Id == servicePackageId, cancellationToken);

            if (servicePackage is null)
                throw new Exception("Không tìm thấy gói dịch vụ tương ứng với gift code.");

            var subscription = UserSubscription.Activate(
                request.PatientId,
                servicePackageId,
                startDate,
                null,
                Guid.Parse(giftCode.Id),
                servicePackage, // ✅ truyền đúng object
                0
            );

            context.UserSubscriptions.Add(subscription);

            // Mark gift code as used
            await promotionService.ConsumeGiftCodeAsync(new ConsumeGiftCodeRequest { GiftCodeId = giftCode.Id });

            await context.SaveChangesAsync(cancellationToken);

            return new ActivateSubscriptionByGiftCodeResult(subscription.Id);
        }
    }
}
