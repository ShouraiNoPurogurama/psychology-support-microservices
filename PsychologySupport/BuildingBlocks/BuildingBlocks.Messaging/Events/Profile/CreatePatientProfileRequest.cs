using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Notification;


namespace BuildingBlocks.Messaging.Events.Profile
{
    public record CreatePatientProfileRequest(
       Guid UserId,
       string? Allergies,
       PersonalityTrait PersonalityTraits,
       ContactInfo ContactInfo
       // DateOnly? BirthDate
   );
}
