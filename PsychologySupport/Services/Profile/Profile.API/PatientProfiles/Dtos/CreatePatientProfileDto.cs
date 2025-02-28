using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Dtos
{
    public record CreatePatientProfileDto(
        Guid UserId,
        string FullName,
        string Gender,
        string Allergies,
        PersonalityTrait PersonalityTraits,
        ContactInfo ContactInfo
    );
}
