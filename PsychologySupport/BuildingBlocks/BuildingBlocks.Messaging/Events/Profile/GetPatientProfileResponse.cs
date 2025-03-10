using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Profile;

public record GetPatientProfileResponse(
    bool PatientExists,
    string FullName,
    UserGender Gender,
    string? Allergies,
    string PersonalityTraits,
    string Address,
    string PhoneNumber,
    string Email);