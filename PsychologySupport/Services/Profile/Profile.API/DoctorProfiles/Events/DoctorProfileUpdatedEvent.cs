using BuildingBlocks.DDD;

namespace Profile.API.DoctorProfiles.Events;

public record DoctorProfileUpdatedEvent(
    Guid Id,
    string? Specialty,
    string? Qualifications,
    int YearsOfExperience,
    string? Bio
) : IDomainEvent;
