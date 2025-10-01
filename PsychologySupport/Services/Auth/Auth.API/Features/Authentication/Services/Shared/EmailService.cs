using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Protos;

namespace Auth.API.Features.Authentication.Services.Shared;

public class EmailService(
    UserManager<User> userManager,
    IConfiguration configuration,
    IPublishEndpoint publishEndpoint,
    IWebHostEnvironment env,
    NotificationService.NotificationServiceClient notificationClient)
    : IEmailService
{
    public async Task SendEmailConfirmationAsync(User user)
    {
        if (await HasSentResetEmailRecentlyAsync(user.Email!))
        {
            throw new RateLimitExceededException(
                "Vui lòng đợi ít nhất 1 phút trước khi gửi lại email xác nhận. Nếu chưa nhận được email, hãy kiểm tra hộp thư rác (spam) hoặc đợi thêm một chút.");
        }

        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var baseUrl = configuration["Mail:ConfirmationUrl"]!;
        var url = string.Format(baseUrl, Uri.EscapeDataString(emailConfirmationToken), Uri.EscapeDataString(user.Email));


        var basePath = Path.Combine(env.ContentRootPath, "Features", "Authentication", "EmailTemplates");
        var confirmTemplatePath = Path.Combine(basePath, configuration["EmailTemplates:ConfirmEmail"]!);

        var confirmBody = RenderTemplate(confirmTemplatePath, new Dictionary<string, string>
        {
            ["ConfirmUrl"] = url,
            ["Year"] = DateTimeOffset.UtcNow.Year.ToString()
        });

        var sendEmailIntegrationEvent = new SendEmailIntegrationEvent(user.Email, "Xác nhận tài khoản", confirmBody);

        user.PhoneNumberConfirmed = true;
        await publishEndpoint.Publish(sendEmailIntegrationEvent);
    }

    public async Task SendPasswordResetEmailAsync(User user)
    {
        if (await HasSentResetEmailRecentlyAsync(user.Email!))
        {
            throw new RateLimitExceededException(
                "Vui lòng đợi ít nhất 1 phút trước khi gửi lại email đổi mật khẩu. Nếu chưa nhận được email, hãy kiểm tra hộp thư rác (spam) hoặc đợi thêm một chút.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrlTemplate = configuration["Mail:PasswordResetUrl"];
        var callbackUrl = string.Format(resetUrlTemplate!, Uri.EscapeDataString(token), Uri.EscapeDataString(user.Email));

        var basePath = Path.Combine(env.ContentRootPath, "Domains", "Authentication", "EmailTemplates");
        var resetTemplatePath = Path.Combine(basePath, configuration["EmailTemplates:ResetPassword"]!);

        var resetBody = RenderTemplate(resetTemplatePath, new Dictionary<string, string>
        {
            ["ResetUrl"] = callbackUrl,
            ["Year"] = DateTimeOffset.UtcNow.Year.ToString()
        });

        var sendEmailEvent = new SendEmailIntegrationEvent(user.Email, "Khôi phục mật khẩu", resetBody);
        await publishEndpoint.Publish(sendEmailEvent);
    }

    private async Task<bool> HasSentResetEmailRecentlyAsync(string email)
    {
        var grpcRequest = new Notification.API.Protos.HasSentEmailRecentlyRequest { Email = email };
        var response = await notificationClient.HasSentEmailRecentlyAsync(grpcRequest);
        return response.IsRecentlySent;
    }

    private string RenderTemplate(string templatePath, Dictionary<string, string> values)
    {
        var template = File.ReadAllText(templatePath);
        foreach (var pair in values)
        {
            template = template.Replace($"{{{{{pair.Key}}}}}", pair.Value);
        }

        return template;
    }
}