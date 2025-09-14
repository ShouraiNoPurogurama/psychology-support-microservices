using BuildingBlocks.DDD;
using BuildingBlocks.Utils;

namespace Profile.API.Models.Pii;

public class PatientOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid SubjectRef { get; private set; }
    public Guid PatientProfileId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    public PersonProfile PersonProfile { get; set; }

    public static PatientOwnerMap Create(Guid subjectRef, Guid patientProfileId)
    {
        ValidationBuilder.Create()
            .When(() => subjectRef == Guid.Empty)
                .WithErrorCode("INVALID_SUBJECT_REF")
                .WithMessage("Reference người dùng không được để trống.")
            .When(() => patientProfileId == Guid.Empty)
                .WithErrorCode("INVALID_PATIENT_PROFILE_ID")
                .WithMessage("ID hồ sơ không được để trống.")
            .ThrowIfInvalid();
        
        var patientOwnerMap = new PatientOwnerMap
        {
            Id = Guid.NewGuid(),
            SubjectRef = subjectRef,
            PatientProfileId = patientProfileId
        };

        return patientOwnerMap;
    }
}