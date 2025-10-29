using Auth.API.Data;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using Auth.API.Features.Encryption.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Google.Apis.Auth;
using MassTransit;

namespace Auth.API.Features.Authentication.Services.Shared;

public class UserProvisioningService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    IPayloadProtector payloadProtector,
    IUserOnboardingService userOnboardingService,
    IPublishEndpoint publishEndpoint)
    : IUserProvisioningService
{
    public async Task<User> FindOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload, string? referralCode)
    {
        var user = await userManager.FindByEmailAsync(payload.Email);

        if (user != null) return await LoadFullUserAsync(user.Id);

        if (user is not null) return user;

        //Tạo user mới
        user = new User
        {
            Email = payload.Email,
            UserName = payload.Email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new InvalidDataException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        await AssignUserRoleAsync(user);

        await CreateUserProfileFromGoogleAsync(user, payload, referralCode);
        
        var onboardingRecord = new UserOnboarding
        {
            User = user,
            PiiCompleted = false,
            PatientProfileCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        authDbContext.UserOnboardings.Add(onboardingRecord);

        await authDbContext.SaveChangesAsync();

        return await LoadFullUserAsync(user.Id);
    }

    private async Task AssignUserRoleAsync(User user)
    {
        var roleResult = await userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded)
            throw new InvalidDataException("Gán vai trò thất bại");
    }

    private async Task CreateUserProfileFromGoogleAsync(User user, GoogleJsonWebSignature.Payload payload, string? referralCode)
    {
        var userRegisteredIntegrationEvent = new UserRegisteredIntegrationEvent(
            SeedPatientProfileId: Guid.NewGuid(),
            SeedSubjectRef: Guid.NewGuid(),
            UserId: user.Id,
            Email: user.Email!,
            PhoneNumber: user.PhoneNumber,
            FullName: payload.Name,
            ReferralCode: referralCode
        );

        await publishEndpoint.Publish(userRegisteredIntegrationEvent);
    }

    private async Task<User> LoadFullUserAsync(Guid userId)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
               .ThenInclude(ur => ur.Role)
            .Include(u => u.Onboarding)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Không tìm thấy người dùng sau khi tạo.");
    }
}