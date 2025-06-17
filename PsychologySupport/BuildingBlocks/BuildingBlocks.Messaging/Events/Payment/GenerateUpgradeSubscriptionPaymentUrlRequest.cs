using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Payment;

public record GenerateUpgradeSubscriptionPaymentUrlRequest
{
    //Subscription
    public Guid SubscriptionId { get; set; }
    public Guid ServicePackageId { get; set; }
        
    public Guid PatientId { get; set; }
    public string PatientEmail { get; set; }
        
    public string? PromoCode { get; set; }
    public Guid? GiftId { get; set; }
        
    //Service Package
    public string Name { get; set; }
    public string Description { get; set; }
    public int DurationDays { get; set; }
    public string? ServicePackageName { get; set; }
    //Payment
    public PaymentMethodName PaymentMethodName { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal FinalPrice { get; set; }
        
    public decimal OldSubscriptionPrice { get; set; } = 0;
};