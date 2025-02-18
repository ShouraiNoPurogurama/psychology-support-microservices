using Profile.API.Common.ValueObjects;

namespace Profile.API.DoctorProfiles.Dtos
{
    public class DoctorProfileCreate
    {
		public string? Gender { get; set; }
		public ContactInfo ContactInfo { get; set; } = default!;

		public string Specialty { get; set; }

		public string Qualifications { get; set; }

		public int YearsOfExperience { get; set; }

		public string Bio { get; set; }

		public float Rating { get; set; }

		public int TotalReviews { get; set; }

	}
}
