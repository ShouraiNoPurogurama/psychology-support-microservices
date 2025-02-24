using BuildingBlocks.Data.Enums;
using Profile.API.Common.ValueObjects;

namespace Profile.API.DoctorProfiles.Dtos
{
    public record CreateDoctorProfileDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public ContactInfo ContactInfo { get; set; }

        public string Specialty { get; set; }

        public string Qualifications { get; set; }

        public int YearsOfExperience { get; set; }

        public string? Bio { get; set; }

    }
}
