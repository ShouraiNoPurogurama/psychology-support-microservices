using System.Text.Json.Serialization;
using BuildingBlocks.DDD;
using Profile.API.MentalDisorders.Models;

namespace Profile.API.PatientProfiles.Models;

public class MedicalHistory : Entity<Guid>
{
    public Guid PatientId { get; set; }

    public string Description { get; set; }

    public DateTimeOffset DiagnosedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public PatientProfile PatientProfile { get; set; }

    [JsonIgnore]
    public ICollection<SpecificMentalDisorder> SpecificMentalDisorders { get; set; } = [];

    [JsonIgnore]
    public ICollection<PhysicalSymptom> PhysicalSymptoms { get; set; } = [];
    
    
}