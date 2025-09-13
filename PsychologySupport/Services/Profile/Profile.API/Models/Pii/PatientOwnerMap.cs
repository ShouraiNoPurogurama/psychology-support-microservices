using BuildingBlocks.DDD;

namespace Profile.API.Models.Pii;

public class PatientOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid SubjectRef { get; set; }
    
    public Guid PatientProfileId { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}