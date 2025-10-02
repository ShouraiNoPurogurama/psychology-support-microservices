using Auth.API.Enums;
using BuildingBlocks.DDD;

namespace Auth.API.Models;

public class UserOnboarding : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public UserOnboardingStatus OnboardingStatus { get; set; } = UserOnboardingStatus.Pending;
    public AliasIssueStatus AliasIssueStatus { get; set; } = AliasIssueStatus.Pending;
    public bool PiiCompleted { get; set; } //e.g. FullName + BirthDate tối thiểu
    public bool PatientProfileCompleted { get; set; } //e.g. JobId tối thiểu
    public bool AliasIssued { get; set; } //Đã phát hành bí danh cho user này chưa
    
    public string[] Missing { get; set; } = []; //["FullName","JobId",...]

    public string? ReasonCode { get; set; }

    public User User { get; init; }
}