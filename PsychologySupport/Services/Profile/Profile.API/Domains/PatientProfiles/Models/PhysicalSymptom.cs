namespace Profile.API.Domains.PatientProfiles.Models;

public class PhysicalSymptom
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
}