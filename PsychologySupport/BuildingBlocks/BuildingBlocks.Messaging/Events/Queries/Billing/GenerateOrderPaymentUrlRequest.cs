using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Queries.Billing
{
    public record GenerateOrderPaymentUrlRequest
    {
        // Order 
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public string ProductCode { get; set; } // PointPackageCode
        public Guid SubjectRef { get; set; }

        // email

        public string? PromoCode { get; set; }
        public Guid? GiftId { get; set; }

        // Point Package
        public string PointPackageName { get; set; }
        public string PointPackageDescription { get; set; }
        public int PointAmount { get; set; }

        //Payment
        public PaymentMethodName PaymentMethodName { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
