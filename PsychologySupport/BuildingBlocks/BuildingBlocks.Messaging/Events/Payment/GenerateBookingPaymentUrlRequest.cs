using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Payment;

public class GenerateBookingPaymentUrlRequest
{
    public Guid BookingId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public int Duration { get; set; }


    public Guid DoctorId { get; set; }
    public string DoctorEmail { get; set; }
    public Guid PatientId { get; set; }
    public string PatientEmail { get; set; }

    //Promotion
    public string? PromoCode { get; set; }
    public Guid? GiftId { get; set; }

    //Payment
    public PaymentMethodName PaymentMethod { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal FinalPrice { get; set; }
}