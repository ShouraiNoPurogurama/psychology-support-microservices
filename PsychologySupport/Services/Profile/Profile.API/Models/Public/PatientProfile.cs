using BuildingBlocks.Data.Common;
using BuildingBlocks.DDD;
using Profile.API.Domains.PatientProfiles.Enum;
using Profile.API.Domains.PatientProfiles.Events;

namespace Profile.API.Models.Public;

public class PatientProfile : AggregateRoot<Guid>
{
    private readonly List<MedicalRecord> _medicalRecords = [];


    public PatientProfile()
    {
    }

    public PatientProfile(Guid userId, string? allergies,
       PersonalityTrait personalityTraits, Guid? jobId)
    {
        UserId = userId;
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        JobId = jobId;
        IsProfileCompleted = CheckProfileCompleted();
    }

    public Guid UserId { get; set; }
    public string? Allergies { get; set; }
    public PersonalityTrait PersonalityTraits { get; set; }
    public Guid? MedicalHistoryId { get; set; }
    public MedicalHistory? MedicalHistory { get; set; }
    public Guid? JobId { get; set; }
    public Job? Job { get; set; }
    public bool IsProfileCompleted { get; set; }

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

    public static PatientProfile Create(Guid userId, string fullName, UserGender gender, string? allergies,
       PersonalityTrait personalityTraits, ContactInfo contactInfo, Guid? jobId, DateOnly? birthDate)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("ID người dùng không được để trống.", nameof(userId));

        var profile = new PatientProfile(userId, allergies, personalityTraits, jobId);
        
        profile.IsProfileCompleted = profile.CheckProfileCompleted();
        return profile;
    }

    public void Update( string? allergies, PersonalityTrait personalityTraits, Guid? jobId)
    {
        Allergies = allergies;
        PersonalityTraits = personalityTraits;
        JobId = jobId;
        IsProfileCompleted = CheckProfileCompleted();
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

    private bool CheckProfileCompleted()
    {
        // return !string.IsNullOrWhiteSpace(FullName)
        //        && ContactInfo.HasEnoughInfo()
        //        //&& ContactInfo.PhoneNumber is not null
        //        && BirthDate.HasValue;
        // // && JobId.HasValue;
        //TODO sửa lại logic này sau khi hoàn thiện phần Pii
        return true;
    }
}