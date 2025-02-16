using System.Text.Json.Serialization;
using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class MedicalHistory : Entity<Guid>
{
    public Guid PatientId { get; set; }
    
    public string Description { get; set; }
    
    public DateTimeOffset DiagnosedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public PatientProfile PatientProfile { get; set; }
    public ICollection<SpecificMentalDisorder> SpecificMentalDisorders { get; set; } = [];
    public ICollection<PhysicalSymptom> PhysicalSymptoms { get; set; } = [];
}