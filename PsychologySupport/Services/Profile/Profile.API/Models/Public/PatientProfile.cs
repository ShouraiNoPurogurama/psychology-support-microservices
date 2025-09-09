using BuildingBlocks.DDD;
using BuildingBlocks.Utils;
using Profile.API.Domains.Public.PatientProfiles.Enum;
using Profile.API.Domains.Public.PatientProfiles.Events;

namespace Profile.API.Models.Public;

public class PatientProfile : AggregateRoot<Guid>
{
    private PatientProfile()
    {
    }

    private PatientProfile(
        Guid subjectRef, 
        string? allergies,
        PersonalityTrait personalityTraits, Guid? jobId)
    {
        // SubjectRef = subjectRef;
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        JobId = jobId;
        IsProfileCompleted = CheckProfileCompleted();
    }

    public Guid SubjectRef { get; private set; }
    public string? Allergies { get; private set; }
    public PersonalityTrait PersonalityTraits { get; private set; }

    public Guid? MedicalHistoryId { get; private set; }
    public MedicalHistory? MedicalHistory { get; private set; }

    public Guid? JobId { get; private set; }
    public Job? Job { get; private set; }

    public bool IsProfileCompleted { get; private set; }


    private readonly List<MedicalRecord> _medicalRecords = [];
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();

    public static PatientProfile Create(Guid subjectRef, string? allergies,
        PersonalityTrait personalityTraits, Guid? jobId)
    {
        var errors = new Dictionary<string, string[]>();
        
        if (subjectRef == Guid.Empty)
            errors.Add("INVALID_SUBJECT_REF", ["Reference người dùng không được để trống."]);
        
        if (allergies != null && allergies.Length > 300)
        {
            errors.Add("INVALID_ALLERGIES", ["Thông tin dị ứng không được vượt quá 300 ký tự."]);
        }

        if (errors.Count > 0)
        {
            throw new CustomValidationException(errors);
        }
        
        var profile = new PatientProfile(subjectRef, allergies, personalityTraits, jobId);
        
        profile.AddDomainEvent(new PatientProfileCreatedEvent(profile.Id));

        return profile;
    }

    public void Update(string? allergies, PersonalityTrait personalityTraits, Guid? jobId)
    {
        ValidationBuilder<PatientProfile>.Create(this)
            .When(_ => allergies is not null && allergies.Length > 300)
            .WithErrorCode("INVALID_ALLERGIES")
            .WithMessage("Thông tin dị ứng không được vượt quá 300 ký tự.")
            .ThrowIfInvalid();
        
        bool updated = allergies != Allergies || personalityTraits != PersonalityTraits || jobId != JobId;
        
        Allergies = allergies;

        PersonalityTraits = personalityTraits;

        JobId = jobId;

        if (updated)
            AddDomainEvent(new PatientProfileUpdatedEvent(SubjectRef, allergies, personalityTraits, jobId));
    }

    public void UpdateAllergies(string? allergies)
    {
        ValidationBuilder<PatientProfile>.Create(this)
            .When(_ => allergies is not null && allergies.Length > 300)
            .WithErrorCode("INVALID_ALLERGIES")
            .WithMessage("Thông tin dị ứng không được vượt quá 300 ký tự.")
            .ThrowIfInvalid();
        
        if (Allergies != allergies)
        {
            Allergies = allergies;
            AddDomainEvent(new PatientAllergiesUpdatedEvent(SubjectRef, allergies));
        }
    }

    public void UpdatePersonalityTraits(PersonalityTrait personalityTraits)
    {
        if (PersonalityTraits != personalityTraits)
        {
            PersonalityTraits = personalityTraits;
            AddDomainEvent(new PatientPersonalityUpdatedEvent(SubjectRef, personalityTraits));
        }
    }

    public void UpdateJob(Guid? jobId)
    {
        if (JobId != jobId)
        {
            JobId = jobId;
            AddDomainEvent(new PatientJobUpdatedEvent(SubjectRef, jobId));
        }
    }

    public void UpdateMedicalRecord(Guid medicalRecordId, Guid doctorId, string notes, MedicalRecordStatus status,
        List<SpecificMentalDisorder> updatedDisorders)
    {
        var medicalRecord = _medicalRecords.FirstOrDefault(mr => mr.Id == medicalRecordId);

        if (medicalRecord is null) throw new KeyNotFoundException("Không tìm thấy hồ sơ y tế tương ứng.");

        medicalRecord.Update(Id, doctorId, MedicalHistoryId, notes, status, updatedDisorders);
    }

    public void UpdateMedicalHistory(string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (MedicalHistory is null)
            throw new InvalidOperationException("Người dùng chưa có tiền sử bệnh. Vui lòng tạo mới trước khi cập nhật.");

        MedicalHistory.Update(description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
    }

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
                "Người dùng chỉ được phép có một tiền sử bệnh. Vui lòng xóa tiền sử hiện tại trước khi thêm mới.");

        MedicalHistory = MedicalHistory.Create(Id, description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
        MedicalHistoryId = MedicalHistory.Id;

        return MedicalHistory;
    }

    public void ReplaceMedicalHistory(MedicalHistory newMedicalHistory)
    {
        if (newMedicalHistory is null)
            throw new ArgumentNullException(nameof(newMedicalHistory), "Tiền sử bệnh không được để trống.");

        MedicalHistory = newMedicalHistory;
        MedicalHistoryId = newMedicalHistory.Id;
    }

    public void RemoveMedicalHistory()
    {
        MedicalHistory = null;
        MedicalHistoryId = null;
    }

    private bool CheckProfileCompleted()
    {
        return JobId.HasValue;
        //TODO sửa lại logic này sau khi hoàn thiện phần Pii
        return true;
    }
}