using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class MedicalRecord : Entity<Guid>
{
    public Guid PatientId { get; set; }
    
    public Guid DoctorId { get; set; }
    
    public Guid MedicalHistoryId { get; set; }

    public string Notes { get; set; }

    public MedicalRecordStatus Status { get; set; }

    public virtual PatientProfile PatientProfile { get; set; }
    
    public ICollection<SpecificMentalDisorder> SpecificMentalDisorders { get; set; }
}