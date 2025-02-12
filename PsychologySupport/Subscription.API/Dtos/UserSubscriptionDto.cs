using Subscription.API.Models;

namespace Subscription.API.Dtos
{
    public record UserSubscriptionDto(
        Guid Id,
        Guid? UserId,
        Guid? ServicePackageId,
        DateTime? StartDate,
        DateTime? EndDate,
        string? Status
    );
}
