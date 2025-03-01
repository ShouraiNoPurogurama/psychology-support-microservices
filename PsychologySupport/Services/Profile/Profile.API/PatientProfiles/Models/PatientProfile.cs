using BuildingBlocks.Data.Enums;
using BuildingBlocks.DDD;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Events;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Models;

public class PatientProfile : AggregateRoot<Guid>
{
    private readonly List<MedicalRecord> _medicalRecords = [];


    public PatientProfile()
    {
    }

    public PatientProfile(Guid userId, string fullName, UserGender gender, string? allergies, PersonalityTrait personalityTraits,
        ContactInfo contactInfo)
    {
        UserId = userId;
        FullName = fullName;
        Gender = gender;
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        ContactInfo = contactInfo;
    }

    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public UserGender Gender { get; set; }
    public string? Allergies { get; set; }
    public PersonalityTrait PersonalityTraits { get; set; }
    public ContactInfo ContactInfo { get; set; } = default!;
    public Guid? MedicalHistoryId { get; set; }
    public MedicalHistory? MedicalHistory { get; set; }

    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    public MedicalRecord AddMedicalRecord(Guid doctorId, string notes, MedicalRecordStatus status,
        List<SpecificMentalDisorder> existingDisorders)
    {
        var medicalRecord = MedicalRecord.Create(Id, doctorId, MedicalHistoryId, notes, status, existingDisorders);

        _medicalRecords.Add(medicalRecord);

        AddDomainEvent(new MedicalRecordAddedEvent(medicalRecord));

        return medicalRecord;
    }

    public MedicalHistory AddMedicalHistory(string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (MedicalHistory is not null)
            throw new InvalidOperationException(
                "A patient can only have one medical history. Remove the existing one before adding a new one.");

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

    public static PatientProfile Create(Guid userId, string fullName, UserGender gender, string? allergies,
        PersonalityTrait personalityTraits, ContactInfo contactInfo)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new PatientProfile(userId, fullName, gender, allergies, personalityTraits, contactInfo);
    }

    public void Update(string fullName, UserGender gender, string allergies, PersonalityTrait personalityTraits,
        ContactInfo contactInfo)
    {
        FullName = fullName;
        Gender = gender;
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        ContactInfo = contactInfo;
    }

    public void UpdateMedicalRecord(Guid medicalRecordId, Guid doctorId, string notes, MedicalRecordStatus status,
        List<SpecificMentalDisorder> updatedDisorders)
    {
        var medicalRecord = _medicalRecords.FirstOrDefault(mr => mr.Id == medicalRecordId);

        if (medicalRecord is null) throw new KeyNotFoundException("Medical record not found.");

        medicalRecord.Update(Id, doctorId, MedicalHistoryId, notes, status, updatedDisorders);
    }

    public void UpdateMedicalHistory(string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (MedicalHistory is null)
            throw new InvalidOperationException("Medical history does not exist. Create a new one before updating.");

        MedicalHistory.Update(description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
    }
}