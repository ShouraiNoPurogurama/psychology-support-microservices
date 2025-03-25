using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace BuildingBlocks.Dtos
{
    public record CreatePatientProfileDto(
        Guid UserId,
        string FullName,
        UserGender Gender,
        string? Allergies,
        PersonalityTrait PersonalityTraits,
        ContactInfo ContactInfo
    );
}
