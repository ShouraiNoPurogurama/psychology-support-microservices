using BuildingBlocks.DDD;
using Payment.Domain.Enums;

namespace Payment.Domain.Models;

public class Payment : AggregateRoot<Guid>
{
    public Guid PatientProfileId { get; set; }
    public decimal TotalAmount { get; set; }

    public Guid? SubscriptionId { get; set; }

    public Guid? BookingId { get; set; }
    public PaymentStatus Status { get; set; }

    public Guid PaymentMethodId { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; }
}