namespace Profile.API.Models.Public;

public class Specialty
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public ICollection<DoctorProfile> DoctorProfiles { get; set; } = [];
}