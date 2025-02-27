namespace Subscription.API.Dtos;

public record CreateUserSubscriptionDto(
    Guid PatientId,  
    Guid ServicePackageId,
    Guid? PromotionCodeId,
    Guid? GiftId,
    DateTime StartDate,
    DateTime EndDate
);