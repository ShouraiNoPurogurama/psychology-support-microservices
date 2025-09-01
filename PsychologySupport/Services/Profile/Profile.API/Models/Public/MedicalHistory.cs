using BuildingBlocks.DDD;

namespace Profile.API.Models.Public;

public class MedicalHistory : AuditableEntity<Guid>
{
    private readonly List<PhysicalSymptom> _physicalSymptoms = [];

    private readonly List<SpecificMentalDisorder> _specificMentalDisorders = [];

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

    public Guid PatientId { get; set; }

    public string Description { get; set; }

    public DateTimeOffset DiagnosedAt { get; set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<SpecificMentalDisorder> SpecificMentalDisorders => _specificMentalDisorders.AsReadOnly();

    public IReadOnlyList<PhysicalSymptom> PhysicalSymptoms => _physicalSymptoms.AsReadOnly();

    internal static MedicalHistory Create(Guid patientId, string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("ID người dùng không được để trống.", nameof(patientId));

        if (diagnosedAt > DateTimeOffset.UtcNow)
            throw new ArgumentException("Thời điểm chẩn đoán phải trước thời gian hiện tại.", nameof(diagnosedAt));

        if (specificMentalDisorders.Count == 0 && physicalSymptoms.Count == 0)
            throw new ArgumentException(
                "Vui lòng chọn ít nhất một rối loạn tâm lý hoặc một triệu chứng thể chất để tiếp tục.",
                nameof(specificMentalDisorders) + ", " + nameof(physicalSymptoms));

        return new MedicalHistory(patientId, description, diagnosedAt, specificMentalDisorders, physicalSymptoms);
    }

    internal void Update(string description, DateTimeOffset diagnosedAt,
        List<SpecificMentalDisorder> specificMentalDisorders, List<PhysicalSymptom> physicalSymptoms)
    {
        if (diagnosedAt > DateTimeOffset.UtcNow)
            throw new ArgumentException("Thời điểm chẩn đoán phải trước thời gian hiện tại.", nameof(diagnosedAt));

        if (specificMentalDisorders.Count == 0 && physicalSymptoms.Count == 0)
            throw new ArgumentException(
                "Vui lòng chọn ít nhất một rối loạn tâm lý hoặc một triệu chứng thể chất để tiếp tục.",
                nameof(specificMentalDisorders) + ", " + nameof(physicalSymptoms));

        Description = description;
        DiagnosedAt = diagnosedAt;

        _specificMentalDisorders.Clear();
        _specificMentalDisorders.AddRange(specificMentalDisorders);

        _physicalSymptoms.Clear();
        _physicalSymptoms.AddRange(physicalSymptoms);
    }
}