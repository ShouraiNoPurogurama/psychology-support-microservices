using BuildingBlocks.DDD;

namespace Profile.API.DoctorProfiles.Events
{
        public record DoctorProfileCreatedEvent(
        Guid UserId,
        string? Gender,
        string? Email,
        string? PhoneNumber,
        string? Specialty,
        DateTimeOffset? CreatedAt
    ) : IDomainEvent;

}
