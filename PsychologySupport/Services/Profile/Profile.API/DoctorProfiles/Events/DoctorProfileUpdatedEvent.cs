using BuildingBlocks.DDD;
using MediatR;

namespace Profile.API.DoctorProfiles.Events;

public record DoctorProfileUpdatedEvent(
    Guid UserId,
    string Gender,
    string Email,
    string PhoneNumber,
    DateTimeOffset? LastModified
);
