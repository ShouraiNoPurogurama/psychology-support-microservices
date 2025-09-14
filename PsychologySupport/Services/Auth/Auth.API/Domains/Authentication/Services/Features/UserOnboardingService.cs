using Auth.API.Data;
using Auth.API.Domains.Authentication.Dtos.Responses;
using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts.Features;
using Auth.API.Enums;
using BuildingBlocks.Exceptions;

namespace Auth.API.Domains.Authentication.Services.Features;

public class UserOnboardingService(AuthDbContext dbContext, ILogger<UserOnboardingService> logger) : IUserOnboardingService
{
    public async Task<bool> MarkPiiOnboardedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Onboarding)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            logger.LogError($"User {userId} was not found, cannot mark PII as onboarded");

            return false;
        }

        user.Onboarding!.PiiCompleted = true;

        if (IsOnboardingComplete(user))
            user.OnboardingStatus = UserOnboardingStatus.Completed;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> MarkPatientOnboardedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Onboarding)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            logger.LogError($"User {userId} was not found, cannot mark Patient as onboarded");

            return false;
        }

        user.Onboarding!.PatientProfileCompleted = true;

        if (IsOnboardingComplete(user))
            user.OnboardingStatus = UserOnboardingStatus.Completed;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
    
    public async Task<UserOnboardingStatusDto> GetOnboardingStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Onboarding)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
            throw new UserNotFoundException();

        var result = new UserOnboardingStatusDto(
            Status: user.OnboardingStatus,
            PiiCompleted: user.Onboarding?.PiiCompleted ?? false,
            PatientProfileCompleted: user.Onboarding?.PatientProfileCompleted  ?? false
        );
        
        return result;
    }

    private bool IsOnboardingComplete(User user) =>
        user.Onboarding?.PiiCompleted == true && user.Onboarding?.PatientProfileCompleted == true;
}