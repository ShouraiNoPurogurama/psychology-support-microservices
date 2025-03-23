namespace Scheduling.API.Dtos
{
    //public record CreateDoctorAvailabilityDto(Guid DoctorId, DateOnly Date, TimeOnly StartTime);

    public record CreateDoctorAvailabilityDto(Guid DoctorId, DateOnly Date, List<TimeOnly> StartTimes);
}
