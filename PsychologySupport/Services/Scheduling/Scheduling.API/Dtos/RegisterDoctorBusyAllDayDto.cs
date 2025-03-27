namespace Scheduling.API.Dtos
{
    public record RegisterDoctorBusyAllDayDto(Guid DoctorId, DateOnly Date);
}
