using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Queries.Profile
{
    public record CreatePatientProfileRequest(
       Guid UserId,
       string? Allergies,
       PersonalityTrait PersonalityTraits,
       ContactInfo ContactInfo
       // DateOnly? BirthDate
   );
}
