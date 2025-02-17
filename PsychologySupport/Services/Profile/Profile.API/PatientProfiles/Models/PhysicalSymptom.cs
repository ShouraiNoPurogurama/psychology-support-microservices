using System.Text.Json.Serialization;

namespace Profile.API.PatientProfiles.Models;

public class PhysicalSymptom
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    [JsonIgnore]
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
}