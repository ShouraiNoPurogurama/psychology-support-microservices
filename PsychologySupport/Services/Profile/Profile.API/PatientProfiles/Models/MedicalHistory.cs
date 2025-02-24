using BuildingBlocks.DDD;
using Profile.API.MentalDisorders.Models;

namespace Profile.API.PatientProfiles.Models;

public class MedicalHistory : Entity<Guid>
{
    public Guid PatientId { get; set; }

    public string Description { get; set; }

    public DateTimeOffset DiagnosedAt { get; set; } = DateTimeOffset.UtcNow;

    private readonly List<SpecificMentalDisorder> _specificMentalDisorders = [];

    public IReadOnlyList<SpecificMentalDisorder> SpecificMentalDisorders => _specificMentalDisorders.AsReadOnly();

    private readonly List<PhysicalSymptom> _physicalSymptoms = [];

    public IReadOnlyList<PhysicalSymptom> PhysicalSymptoms => _physicalSymptoms.AsReadOnly();

    private MedicalHistory(Guid patientId, string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        Description = description;
        DiagnosedAt = diagnosedAt;
        _specificMentalDisorders = specificMentalDisorders;
        _physicalSymptoms = physicalSymptoms;
    }

    public MedicalHistory()
    {
        
    }

    internal static MedicalHistory Create(Guid patientId, string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID cannot be empty.", nameof(patientId));

        if (diagnosedAt > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Diagnose time must be before current time.", nameof(diagnosedAt));
        }

        if (specificMentalDisorders.Count == 0 && physicalSymptoms.Count == 0)
        {
            throw new ArgumentException("Specific mental disorders and Physical symptoms cannot be both empty", nameof(specificMentalDisorders) + ", " + nameof(physicalSymptoms));
        }

        return new MedicalHistory(patientId, description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
    }

    internal void Update(string description, DateTimeOffset diagnosedAt,
       List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (diagnosedAt > DateTimeOffset.UtcNow)
            throw new ArgumentException("Diagnose time must be before current time.", nameof(diagnosedAt));

        if (specificMentalDisorders.Count == 0 && physicalSymptoms.Count == 0)
            throw new ArgumentException("Specific mental disorders and Physical symptoms cannot be both empty.", nameof(specificMentalDisorders) + ", " + nameof(physicalSymptoms));

        Description = description;
        DiagnosedAt = diagnosedAt;

        _specificMentalDisorders.Clear();
        _specificMentalDisorders.AddRange(specificMentalDisorders);

        _physicalSymptoms.Clear();
        _physicalSymptoms.AddRange(physicalSymptoms);
    }
}