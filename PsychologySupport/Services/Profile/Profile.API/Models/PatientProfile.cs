using BuildingBlocks.DDD;

namespace Profile.API.Models;

public class PatientProfile : Entity<Guid>
{
    public Guid UserId { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; } // Can be stored as JSON

    public string? Address { get; set; }

    public string PhoneNumber { get; set; } = default!;

    public string? EmergencyContact { get; set; }

}