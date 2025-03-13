using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Profile;

public record GetDoctorProfileResponse(
    bool DoctorExists,
    Guid Id,
    string FullName,
    UserGender Gender,
    string Address,
    string PhoneNumber,
    string Email);