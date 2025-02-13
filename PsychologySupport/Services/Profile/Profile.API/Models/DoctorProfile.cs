using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class DoctorProfile : Entity<Guid>
{
    public Guid UserId { get; set; }

    public string Specialty { get; set; } = default!;

    public string Qualifications { get; set; } = default!;

    public int YearsOfExperience { get; set; }

    public string Bio { get; set; } = default!;

    public string? ProfilePicture { get; set; }

    public float Rating { get; set; }

    public int TotalReviews { get; set; }

}