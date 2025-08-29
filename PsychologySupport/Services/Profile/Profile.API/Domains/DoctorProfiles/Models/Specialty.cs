namespace Profile.API.Domains.DoctorProfiles.Models;

public class Specialty
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<DoctorProfile> DoctorProfiles { get; set; } = [];
}