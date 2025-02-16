using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class DoctorProfile : Entity<Guid>
{
    public Guid UserId { get; set; }
    
    public string? Gender { get; set; }
    
    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }

    public string Specialty { get; set; }
    
    public string Qualifications { get; set; }
    
    public int YearsOfExperience { get; set; }
    
    public string Bio { get; set; }
    
    public float Rating { get; set; }
    
    public int TotalReviews { get; set; }
    
    public ICollection<MedicalRecord> MedicalRecords { get; set; }
}