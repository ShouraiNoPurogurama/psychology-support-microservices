namespace Profile.API.Domains.PatientProfiles.Dtos
{
    public class JobDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = default!;
        public string EducationLevel { get; set; } = default!; 
    }
}
