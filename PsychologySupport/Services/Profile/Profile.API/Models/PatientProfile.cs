using System.Text.Json.Serialization;
using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class PatientProfile : Entity<Guid>
{
    public Guid UserId { get; set; }
    
    public string? Gender { get; set; }
    
    public string? Allergies { get; set; }
    
    public PersonalityTrait PersonalityTraits { get; set; }
    
    public Guid MedicalHistoryId { get; set; }
    
    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }
    
    [JsonIgnore]
    public virtual MedicalHistory MedicalHistory { get; set; }
    
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

}