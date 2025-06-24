using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Profile;

public record GetPatientProfileResponse(
    bool PatientExists,
    Guid Id,
    string FullName,
    UserGender Gender,
    string? Allergies,
    string PersonalityTraits,
    string Address,
    string PhoneNumber,
    string Email,
    Guid UserId,
    bool IsProfileCompleted
    );