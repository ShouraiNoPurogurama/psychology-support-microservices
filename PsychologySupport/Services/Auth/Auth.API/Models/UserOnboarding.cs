using Auth.API.Enums;
using BuildingBlocks.DDD;

namespace Auth.API.Models;

public class UserOnboarding : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public UserOnboardingStatus Status { get; set; } = UserOnboardingStatus.Pending;
    
    public bool PiiCompleted { get; set; } //e.g. FullName + BirthDate tối thiểu
    public bool PatientProfileCompleted { get; set; } //e.g. JobId tối thiểu
    
    public string[] Missing { get; set; } = []; //["FullName","JobId",...]

    public string? ReasonCode { get; set; }

    public User User { get; init; }
}