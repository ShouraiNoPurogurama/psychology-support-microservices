using Auth.API.Data;
using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using Auth.API.Features.Encryption.Dtos;
using Auth.API.Features.Encryption.ServiceContracts;
using BuildingBlocks.Constants;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;
using Mapster;
using MassTransit;

namespace Auth.API.Features.Authentication.Services.Features;

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
        var existingUser = await userManager.Users.AnyAsync(u =>
            u.PhoneNumber == registerRequest.PhoneNumber || u.Email == registerRequest.Email);

        if (existingUser) throw new ConflictException("Email hoặc số điện thoại đã tồn tại trong hệ thống");

        await using var transaction = await authDbContext.Database.BeginTransactionAsync();
        User user;

        try
        {
            user = registerRequest.Adapt<User>();

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

            var dto = new PendingSeedDto(
                registerRequest.FullName,
                Email: registerRequest.Email,
                PhoneNumber: registerRequest.PhoneNumber);

            pendingVerificationUser.PayloadProtected = payloadProtector.Protect(dto);

            authDbContext.PendingVerificationUsers.Add(pendingVerificationUser);

            await AssignUserRoleAsync(user);
            
            await authDbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new InvalidDataException($"Đăng ký thất bại. Vui lòng thử lại hoặc liên hệ quản trị viên hệ thống.");
        }

        try
        {
            await emailService.SendEmailConfirmationAsync(user);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Tạo tài khoản thành công nhưng không thể gửi email xác thực.", ex);
        }

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

        if (user.EmailConfirmed)
            throw new ConflictException("Email của bạn đã được xác nhận trước đó.");

        var result = await userManager.ConfirmEmailAsync(user, token);

        string status = result.Succeeded ? "success" : "failed";
        string message;

        if (result.Succeeded)
        {
            CreateUserOnboarding(user);

            await RaiseUserRegisteredEventAsync(user);

            await authDbContext.SaveChangesAsync();

            message = "Xác nhận email thành công.";
        }
        else
        {
            logger.LogError(
                $"*** Xác nhận email thất bại cho user {user.Id}. \n[Details] {string.Join("; ", result.Errors.Select(e => e.Description))}");
            message = $"Xác nhận email thất bại.";
        }

        var baseRedirectUrl = configuration["Mail:ConfirmationRedirectUrl"];
        var redirectUrl = $"{baseRedirectUrl}?status={status}&message={Uri.EscapeDataString(message)}";

        return redirectUrl;
    }

    private void CreateUserOnboarding(User user)
    {
        var onboardingRecord = new UserOnboarding
        {
            User = user,
            PiiCompleted = false,
            PatientProfileCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        authDbContext.UserOnboardings.Add(onboardingRecord);
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
            SeedPatientProfileId: Guid.NewGuid(),
            SeedSubjectRef: Guid.NewGuid(),
            UserId: user.Id,
            Email: pendingSeedDto.Email,
            PhoneNumber: pendingSeedDto.PhoneNumber,
            FullName: pendingSeedDto.FullName
        );

        await publishEndpoint.Publish(userRegisteredIntegrationEvent);

        pendingUser.ProcessedAt = DateTimeOffset.UtcNow;
    }
}