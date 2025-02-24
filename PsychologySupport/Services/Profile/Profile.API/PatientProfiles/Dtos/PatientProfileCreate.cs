using Profile.API.Common.ValueObjects;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Dtos
{
    public class PatientProfileCreate
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? Allergies { get; set; }
        public PersonalityTrait PersonalityTraits { get; set; }
        public ContactInfo ContactInfo { get; set; } = default!;
    }
}
