using Scheduling.API.Data.Common;

namespace Scheduling.API.Dtos
{
    public record BookingDto(
        string BookingCode,
        Guid DoctorId,
        Guid PatientId,
        DateOnly Date,
        TimeOnly StartTime,
        int Duration,
        decimal Price,
        Guid? PromoCodeId,
        Guid? GiftCodeId,
        BookingStatus Status
    );
}
