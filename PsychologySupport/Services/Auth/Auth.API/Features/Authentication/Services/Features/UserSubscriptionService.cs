using Auth.API.Common.Authentication;
using Auth.API.Data;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Microsoft.EntityFrameworkCore;
using Pii.API.Protos;

namespace Auth.API.Features.Authentication.Services.Features;

public class UserSubscriptionService(
    AuthDbContext dbContext,
    PiiService.PiiServiceClient piiClient,
    ICurrentActorAccessor actorAccessor,
    ILogger<UserSubscriptionService> logger) : IUserSubscriptionService
{
    public async Task<bool> UpdateSubscriptionPlanNameAsync(Guid subjectRef, string subscriptionPlanName, DateTimeOffset validFrom, DateTimeOffset validTo)
    {
        // Resolve userId từ subjectRef qua PiiService
        var resolveResponse = await piiClient.ResolveUserIdBySubjectRefAsync(
            new ResolveUserIdBySubjectRefRequest { SubjectRef = subjectRef.ToString() });

        if (string.IsNullOrEmpty(resolveResponse.UserId) || !Guid.TryParse(resolveResponse.UserId, out var userId))
        {
            logger.LogError("Failed to resolve userId from subjectRef {SubjectRef}", subjectRef);
            throw new UserNotFoundException();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            logger.LogError("User {UserId} was not found when updating subscription plan", userId);
            throw new UserNotFoundException();
        }

        user.SubscriptionPlanName = subscriptionPlanName;
        user.ValidFrom = validFrom;
        user.ValidTo = validTo;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Updated subscription plan for user {UserId} (subjectRef {SubjectRef}) to {PlanName}",
            userId, subjectRef, subscriptionPlanName);

        return true;
    }

    public async Task<bool> RemoveExpiredSubscriptionAsync(Guid patientId)
    {
        // Resolve userId từ patientId qua PiiService
        var resolveResponse = await piiClient.ResolveUserIdByPatientIdAsync(
            new ResolveUserIdByPatientIdRequest { PatientId = patientId.ToString() });

        if (string.IsNullOrEmpty(resolveResponse.UserId) ||
            !Guid.TryParse(resolveResponse.UserId, out var userId))
        {
            logger.LogError("Failed to resolve userId from patientId {PatientId}", patientId);
            throw new UserNotFoundException();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            logger.LogError("User {UserId} was not found when removing expired subscription", userId);
            throw new UserNotFoundException();
        }

        if (string.IsNullOrEmpty(user.SubscriptionPlanName))
        {
            logger.LogInformation("User {UserId} already has no subscription plan set", userId);
            return false;
        }

        user.SubscriptionPlanName = "Free Plan";
        user.ValidFrom = DateTimeOffset.UtcNow;
        user.ValidTo = null;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Removed expired subscription for user {UserId} (patientId {PatientId})",
            userId, patientId);

        return true;
    }

    public async Task<bool> ActivateFreeTrialAsync(CancellationToken cancellationToken = default)
    {
        var subjectRef = actorAccessor.GetRequiredSubjectRef();

        // Resolve userId từ subjectRef qua PiiService
        var resolveResponse = await piiClient.ResolveUserIdBySubjectRefAsync(
            new ResolveUserIdBySubjectRefRequest { SubjectRef = subjectRef.ToString() },
            cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(resolveResponse.UserId) || !Guid.TryParse(resolveResponse.UserId, out var userId))
        {
            logger.LogError("Failed to resolve userId from subjectRef {SubjectRef}", subjectRef);
            throw new UserNotFoundException();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            logger.LogError("User {UserId} was not found when activating Free Trial", userId);
            throw new UserNotFoundException();
        }

        if (user.IsFreeTrialUsed)
        {
            logger.LogWarning("User {UserId} has already used Free Trial", userId);
            return false; // Không thể kích hoạt lại
        }

        user.SubscriptionPlanName = "Free Trial";
        user.ValidFrom = DateTimeOffset.UtcNow;
        user.ValidTo = DateTimeOffset.UtcNow.AddDays(3);
        user.IsFreeTrialUsed = true;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Activated Free Trial for user {UserId} from {ValidFrom} to {ValidTo}",
            userId, user.ValidFrom, user.ValidTo);

        return true;
    }
}
