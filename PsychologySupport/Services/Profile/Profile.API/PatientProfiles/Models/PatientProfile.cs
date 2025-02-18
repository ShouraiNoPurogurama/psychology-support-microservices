using System.Text.Json.Serialization;
using BuildingBlocks.DDD;
using Profile.API.Common.ValueObjects;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Events;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Models;

public class PatientProfile : Aggregate<Guid>
{
    public Guid UserId { get; set; }

    public string? Gender { get; set; }

    public string? Allergies { get; set; }

    public PersonalityTrait PersonalityTraits { get; set; }

    public Guid? MedicalHistoryId { get; set; }
    
    public ContactInfo ContactInfo { get; set; } = default!;

    [JsonIgnore]
    public virtual MedicalHistory? MedicalHistory { get; set; }
    
    private readonly List<MedicalRecord> _medicalRecords = [];


    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    public MedicalRecord AddMedicalRecord(Guid patientId, Guid doctorId, Guid? medicalHistoryId,
        string notes, MedicalRecordStatus status,
        List<SpecificMentalDisorder> existingDisorders)
    {
        var medicalRecord = MedicalRecord.Create(patientId, doctorId, medicalHistoryId, notes, status, existingDisorders);
        
        AddDomainEvent(new MedicalRecordAddedEvent(medicalRecord));

        _medicalRecords.Add(medicalRecord);

        return medicalRecord;
    }
}