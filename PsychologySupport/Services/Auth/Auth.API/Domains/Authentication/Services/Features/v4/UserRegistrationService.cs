using Auth.API.Data;
using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts.Features.v4;
using Auth.API.Domains.Authentication.ServiceContracts.Shared;
using Auth.API.Domains.Encryption.Dtos;
using Auth.API.Domains.Encryption.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Mapster;
using MassTransit;

namespace Auth.API.Domains.Authentication.Services.Features.v4;

public class UserRegistrationService(
    UserManager<User> userManager,
    AuthDbContext authDbContext,
    ILogger<UserRegistrationService> logger,
    IConfiguration configuration,
    IPayloadProtector payloadProtector,
    IPublishEndpoint publishEndpoint,
    IEmailService emailService)
    : IUserRegistrationService
{
    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await userManager.FindByEmailAsync(registerRequest.Email);
        existingUser ??= await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerRequest.PhoneNumber);

        if (existingUser is not null) throw new ConflictException("Email hoặc số điện thoại đã tồn tại trong hệ thống");

        var user = registerRequest.Adapt<User>();
        user.Email = user.UserName = registerRequest.Email;

        var result = await userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(ie => ie.Description));
            throw new InvalidDataException($"Đăng ký thất bại: {errors}");
        }

        //Tạo pending verification
        var pendingVerificationUser = new PendingVerificationUser
        {
            UserId = user.Id
        };

        var dto = new PendingSeedDto(registerRequest.FullName, registerRequest.Gender, registerRequest.BirthDate,
            new BuildingBlocks.Data.Common.ContactInfo
            {
                Address = "None",
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
            });

        pendingVerificationUser.PayloadProtected = payloadProtector.Protect(dto);

        authDbContext.PendingVerificationUsers.Add(pendingVerificationUser);

        await authDbContext.SaveChangesAsync();

        await AssignUserRoleAsync(user);
        await emailService.SendEmailConfirmationAsync(user);

        return true;
    }

    public async Task<string> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest)
    {
        var token = confirmEmailRequest.Token;
        var email = confirmEmailRequest.Email;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email hoặc Token bị thiếu.");

        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        var result = await userManager.ConfirmEmailAsync(user, token);

        string status = result.Succeeded ? "success" : "failed";
        string message;

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            await RaiseUserRegisteredEventAsync(user);

            message = "Xác nhận email thành công.";
        }
        else
        {
            logger.LogError($"*** Xác nhận email thất bại cho user {user.Id}. \n[Details] {string.Join("; ", result.Errors.Select(e => e.Description))}");
            message = $"Xác nhận email thất bại.";
        }

        var baseRedirectUrl = configuration["Mail:ConfirmationRedirectUrl"];
        var redirectUrl = $"{baseRedirectUrl}?status={status}&message={Uri.EscapeDataString(message)}";

        return redirectUrl;
    }
    
    private async Task AssignUserRoleAsync(User user)
    {
        var roleResult = await userManager.AddToRoleAsync(user, Roles.UserRole);
        if (!roleResult.Succeeded)
            throw new InvalidDataException("Gán vai trò thất bại");
    }

    private async Task RaiseUserRegisteredEventAsync(User user)
    {
        var pendingUser = await authDbContext.PendingVerificationUsers
                              .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ProcessedAt == null)
                          ?? throw new NotFoundException("Không tìm thấy dữ liệu người đùng để tạo profile",
                              nameof(PendingVerificationUser));

        var pendingSeedDto = payloadProtector.Unprotect<PendingSeedDto>(pendingUser.PayloadProtected);

        var userRegisteredIntegrationEvent = new UserRegisteredIntegrationEvent(
            SeedSubjectRef: Guid.NewGuid(),
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