using BuildingBlocks.DDD;
using Profile.API.Common.ValueObjects;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Events
{
    public record PatientProfileCreatedEvent(
        Guid UserId,
        string? Gender,
        string? Email,
        string? PhoneNumber,
        string? Allergies,
        DateTimeOffset? CreatedAt
    ) : IDomainEvent;
}
