using BuildingBlocks.DDD;

namespace Profile.API.DoctorProfiles.Events;

public record DoctorProfileUpdatedEvent(
    Guid UserId,
    string Gender,
    string Email,
    string PhoneNumber,
    DateTimeOffset? LastModified
) : IDomainEvent;
