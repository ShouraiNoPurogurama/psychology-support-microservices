using Auth.API.Enums;

namespace Auth.API.Models;

public class User : IdentityUser<Guid>
{
    public string? FirebaseUserId { get; set; }
    public virtual ICollection<Device> Devices { get; set; } = [];
    public UserOnboardingStatus OnboardingStatus { get; set; } = UserOnboardingStatus.Pending;
    
    public AliasIssueStatus AliasIssueStatus { get; set; } = AliasIssueStatus.Pending;
    
    public virtual UserOnboarding? Onboarding { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; }
}