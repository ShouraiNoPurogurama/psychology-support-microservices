using BuildingBlocks.DDD;

namespace Profile.API.PatientProfiles.Events
{
    public record PatientProfileUpdatedEvent(
        Guid UserId,
        string FullName,
        string Gender,
        string Email,
        string PhoneNumber,
        DateTimeOffset? LastModified
    );
}
