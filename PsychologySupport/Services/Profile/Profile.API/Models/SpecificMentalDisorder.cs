namespace Profile.API.Models;

public class SpecificMentalDisorder
{
    public Guid Id { get; set; }
    
    public Guid MentalDisorderId { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    public MentalDisorder MentalDisorder { get; set; }
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = [];
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = [];

}