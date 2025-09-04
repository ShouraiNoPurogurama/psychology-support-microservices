namespace BuildingBlocks.Messaging.Events.Queries.Scheduling;

public record ValidateBookingRequest(
    Guid BookingId,
    Guid DoctorId,
    Guid PatientId,
    DateOnly Date,
    TimeOnly StartTime,
    int Duration,
    decimal FinalPrice,
    string? PromoCode,
    Guid? GiftCodeId
);