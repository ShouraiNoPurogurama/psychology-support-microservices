using System.Text.Json.Serialization;
using BuildingBlocks.DDD;
using Profile.API.Common.ValueObjects;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Events;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Models;

public class PatientProfile : AggregateRoot<Guid>
{
    public Guid UserId { get;  set; }
    public string FullName { get; set; }
    public string? Gender { get;  set; }
    public string? Allergies { get;  set; }
    public PersonalityTrait PersonalityTraits { get;  set; }
    public ContactInfo ContactInfo { get;  set; } = default!;
    public Guid? MedicalHistoryId { get;  set; }
    public MedicalHistory? MedicalHistory { get;  set; }

    private readonly List<MedicalRecord> _medicalRecords = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();
    public PatientProfile() { } 

    public PatientProfile(Guid userId, string? gender, string? allergies, PersonalityTrait personalityTraits, ContactInfo contactInfo)
    {
        UserId = userId;
        Gender = gender;
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        ContactInfo = contactInfo;
    }

    public MedicalRecord AddMedicalRecord(Guid doctorId, string notes, MedicalRecordStatus status, List<SpecificMentalDisorder> existingDisorders)
    {
        var medicalRecord = MedicalRecord.Create(Id, doctorId, MedicalHistoryId, notes, status, existingDisorders);
        
        _medicalRecords.Add(medicalRecord);
        
        AddDomainEvent(new MedicalRecordAddedEvent(medicalRecord));
        
        return medicalRecord;
    }

    public MedicalHistory AddMedicalHistory(string description, DateTimeOffset diagnosedAt, List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (MedicalHistory is not null)
        {
            throw new InvalidOperationException("A patient can only have one medical history. Remove the existing one before adding a new one.");
        }

        MedicalHistory = MedicalHistory.Create(Id, description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
        MedicalHistoryId = MedicalHistory.Id;

        return MedicalHistory;
    }

    public void ReplaceMedicalHistory(MedicalHistory newMedicalHistory)
    {
        if (newMedicalHistory is null)
            throw new ArgumentNullException(nameof(newMedicalHistory), "Medical history cannot be null.");

        MedicalHistory = newMedicalHistory;
        MedicalHistoryId = newMedicalHistory.Id;
    }

    public void RemoveMedicalHistory()
    {
        MedicalHistory = null;
        MedicalHistoryId = null;
    }

    public static PatientProfile Create(Guid userId, string? gender, string? allergies, PersonalityTrait personalityTraits, ContactInfo contactInfo)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new PatientProfile(userId, gender, allergies, personalityTraits, contactInfo);
    }
}