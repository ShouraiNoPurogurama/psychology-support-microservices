using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Payment
{
    public record UserSubscriptionCreatedIntegrationEvent : IntegrationEvents
    {
        //Subscription
        public Guid SubscriptionId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? PromotionCodeId { get; set; }
        public Guid? GiftId { get; set; }
        
        //Service Package
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationDays { get; set; }
        public Guid ImageId { get; set; }
        
        //Payment
        public PaymentMethodName PaymentMethodName { get; set; }
        public decimal FinalPrice { get; set; }
    };
}