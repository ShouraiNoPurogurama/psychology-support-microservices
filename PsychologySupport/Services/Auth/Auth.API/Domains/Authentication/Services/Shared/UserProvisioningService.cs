using Auth.API.Data;
using Auth.API.Domains.Authentication.ServiceContracts.Shared;
using Auth.API.Domains.Encryption.Dtos;
using Auth.API.Domains.Encryption.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Google.Apis.Auth;
using MassTransit;

namespace Auth.API.Domains.Authentication.Services.Shared;

public class UserProvisioningService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    IPayloadProtector payloadProtector,
    IPublishEndpoint publishEndpoint)
    : IUserProvisioningService
{
    public async Task<User> FindOrCreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var user = await userManager.FindByEmailAsync(payload.Email);

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

        await CreateUserProfileAsync(user);

        return user;
    }

    private async Task AssignUserRoleAsync(User user)
    {
        var roleResult = await userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded)
            throw new InvalidDataException("Gán vai trò thất bại");
    }

    private async Task CreateUserProfileAsync(User user)
    {
        var pendingUser = await authDbContext.PendingVerificationUsers
                              .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ProcessedAt == null)
                          ?? throw new NotFoundException("Không tìm thấy dữ liệu người đùng để tạo profile",
                              nameof(PendingVerificationUser));

        var pendingSeedDto = payloadProtector.Unprotect<PendingSeedDto>(pendingUser.PayloadProtected);

        var userRegisteredIntegrationEvent = new UserRegisteredIntegrationEvent(
            UserId: user.Id,
            Email: pendingSeedDto.ContactInfo!.Email,
            PhoneNumber: pendingSeedDto.ContactInfo.PhoneNumber,
            Address: pendingSeedDto.ContactInfo.Address,
            FullName: pendingSeedDto.FullName,
            BirthDate: pendingSeedDto.BirthDate,
            Gender: pendingSeedDto.Gender
        );

        await publishEndpoint.Publish(userRegisteredIntegrationEvent);

        pendingUser.ProcessedAt = DateTimeOffset.UtcNow;

        await authDbContext.SaveChangesAsync();
    }
}