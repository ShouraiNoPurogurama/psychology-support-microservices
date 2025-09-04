using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile
{
    public record DoctorProfileCreatedRequestEvent
    (
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber,
        string Password
    );
}
