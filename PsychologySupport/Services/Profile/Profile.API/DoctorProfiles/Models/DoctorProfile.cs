using BuildingBlocks.DDD;
using Profile.API.Common.ValueObjects;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.DoctorProfiles.Models;

public class DoctorProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    
    public string FullName { get; set; }
    
    public string? Gender { get; set; }
    public ContactInfo ContactInfo { get; set; } = default!;

    public string Specialty { get; set; }
    
    public string Qualifications { get; set; }
    
    public int YearsOfExperience { get; set; }
    
    public string Bio { get; set; }
    
    public float Rating { get; set; }
    
    public int TotalReviews { get; set; }
    public ICollection<MedicalRecord> MedicalRecords { get; set; }
}