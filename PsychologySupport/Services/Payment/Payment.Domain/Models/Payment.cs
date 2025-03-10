using BuildingBlocks.DDD;
using BuildingBlocks.Enums;
using Payment.Domain.Enums;
using Payment.Domain.Events;

namespace Payment.Domain.Models;

public class Payment : AggregateRoot<Guid>
{
    public Guid PatientProfileId { get; set; }
    public decimal TotalAmount { get; set; }

    public Guid? SubscriptionId { get; set; }
    public Guid? BookingId { get; set; }

    public PaymentStatus Status { get; set; }
    public PaymentType PaymentType { get; set; }
    public Guid PaymentMethodId { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; }


    private readonly List<PaymentDetail> _paymentDetails = [];
    public IReadOnlyList<PaymentDetail> PaymentDetails => _paymentDetails.AsReadOnly();


    private Payment()
    {
    }

    public static Payment Create(
        Guid id,
        Guid patientProfileId,
        string patientEmail,
        PaymentType paymentType,
        Guid paymentMethodId,
        PaymentMethod paymentMethod,
        decimal finalPrice,
        Guid? subscriptionId = null,
        Guid? bookingId = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Payment ID cannot be empty.", nameof(id));
        if (patientProfileId == Guid.Empty)
            throw new ArgumentException("PatientProfileId cannot be empty.", nameof(patientProfileId));
        if (paymentMethodId == Guid.Empty)
            throw new ArgumentException("PaymentMethodId cannot be empty.", nameof(paymentMethodId));

        var payment = new Payment
        {
            Id = id,
            PatientProfileId = patientProfileId,
            SubscriptionId = subscriptionId,
            BookingId = bookingId,
            TotalAmount = finalPrice,
            Status = PaymentStatus.Pending,
            PaymentType = paymentType,
            PaymentMethodId = paymentMethodId
        };

        return payment;
    }

    public void AddPaymentDetail(PaymentDetail paymentDetail)
    {
        ArgumentNullException.ThrowIfNull(paymentDetail);

        _paymentDetails.Add(paymentDetail);
    }

    public void AddFailedPaymentDetail(PaymentDetail paymentDetail, string patientEmail, string? promoCode, Guid? giftId)
    {
        _paymentDetails.Add(paymentDetail.MarkAsFailed());

        switch (PaymentType)
        {
            case PaymentType.BuySubscription:
            {
                var paymentDetailFailedEvent =
                    new PaymentDetailFailedEvent(SubscriptionId!.Value, patientEmail, promoCode, giftId, TotalAmount);

                AddDomainEvent(paymentDetailFailedEvent);
                break;
            }
            case PaymentType.Booking:
                //Todo Handle booking logic
                break;
            default:
                break;
        }
    }

    public void MarkAsCompleted(string patientEmail)
    {
        if (_paymentDetails.All(pd => pd.Status == PaymentDetailStatus.Completed))
        {
            Status = PaymentStatus.Completed;
        }

        switch (PaymentType)
        {
            case PaymentType.BuySubscription:
            {
                var paymentCompletedEvent = new SubscriptionPaymentCompletedEvent(
                    SubscriptionId!.Value, patientEmail, TotalAmount);

                AddDomainEvent(paymentCompletedEvent);
                break;
            }
            case PaymentType.Booking:
                //Todo Handle booking logic
                break;
            default:
                break;
        }
    }
}