namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription
{
    public class CreateGiftCodeEvent
    {
        public string PatientId { get; set; } = default!;
        public string PromotionId { get; set; } = default!;
    }

}
