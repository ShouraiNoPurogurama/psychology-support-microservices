using BuildingBlocks.DDD;
using Profile.API.DoctorProfiles.Models;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Models;

public class MedicalRecord : AuditableEntity<Guid>
{
    private readonly List<SpecificMentalDisorder> _specificMentalDisorders = [];

    public MedicalRecord()
    {
    }

    private MedicalRecord(Guid id, Guid patientId, Guid doctorId, Guid? medicalHistoryId,
        string notes, MedicalRecordStatus status, List<SpecificMentalDisorder> disorders)
    {
        Id = id;
        PatientProfileId = patientId;
        DoctorProfileId = doctorId;
        MedicalHistoryId = medicalHistoryId;
        Notes = notes;
        Status = status;
        _specificMentalDisorders = disorders;
    }

    public Guid PatientProfileId { get; set; }

    public Guid DoctorProfileId { get; set; }

    public Guid? MedicalHistoryId { get; set; }

    public string Notes { get; set; } = default!;

    public MedicalRecordStatus Status { get; set; }

    public MedicalHistory? MedicalHistory { get; set; }

    public virtual PatientProfile PatientProfile { get; set; }

    public virtual DoctorProfile DoctorProfile { get; set; }

    public IReadOnlyList<SpecificMentalDisorder> SpecificMentalDisorders => _specificMentalDisorders.AsReadOnly();

    internal static MedicalRecord Create(Guid patientId, Guid doctorId, Guid? medicalHistoryId,
        string notes, MedicalRecordStatus status,
        List<SpecificMentalDisorder> disorders)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("ID người dùng không được để trống.", nameof(patientId));

        if (doctorId == Guid.Empty)
            throw new ArgumentException("ID bác sĩ không được để trống.", nameof(doctorId));

        if (medicalHistoryId == Guid.Empty)
            throw new ArgumentException("ID tiền sử bệnh không được để trống.", nameof(medicalHistoryId));

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Ghi chú không được để trống.", nameof(notes));

        return new MedicalRecord(Guid.NewGuid(), patientId, doctorId, medicalHistoryId, notes, status, disorders);
    }

    public void Update(Guid patientId, Guid doctorId, Guid? medicalHistoryId,
        string notes, MedicalRecordStatus status, List<SpecificMentalDisorder> disorders)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("ID người dùng không được để trống.", nameof(patientId));

        if (doctorId == Guid.Empty)
            throw new ArgumentException("ID bác sĩ không được để trống.", nameof(doctorId));

        if (medicalHistoryId == Guid.Empty)
            throw new ArgumentException("ID tiền sử bệnh không được để trống.", nameof(medicalHistoryId));

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Ghi chú không được để trống.", nameof(notes));


        PatientProfileId = patientId;
        DoctorProfileId = doctorId;
        MedicalHistoryId = medicalHistoryId;
        Notes = notes;
        Status = status;

        _specificMentalDisorders.Clear();
        _specificMentalDisorders.AddRange(disorders);
    }
}