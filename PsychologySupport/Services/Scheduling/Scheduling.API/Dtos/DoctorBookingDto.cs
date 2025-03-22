namespace Scheduling.API.Dtos
{
    public record DoctorBookingDto(Guid DoctorId, string FullName, int TotalBookings);

}
