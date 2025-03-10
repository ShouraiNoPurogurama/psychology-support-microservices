using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Profile;

public record GetDoctorProfileResponse(
    bool DoctorExists,
    string FullName,
    UserGender Gender,
    string Address,
    string PhoneNumber,
    string Email);