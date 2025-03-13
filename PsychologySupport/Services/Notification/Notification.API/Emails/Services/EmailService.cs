using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Notification.API.Data;
using Notification.API.Emails.Configurations;
using Notification.API.Emails.ServiceContracts;

namespace Notification.API.Emails.Services;

public class EmailService(
    IOptions<AppSettings> appsettingsOptions,
    NotificationDbContext dbContext
    ) : IEmailService
{
    private readonly AppSettings _appSettings = appsettingsOptions.Value;
    private readonly EmailConfiguration _emailConfiguration = appsettingsOptions.Value.Features.Email;
    
    public async Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken)
    {
        var trackId = OnSendEmail(emailMessageDto);

        var emailTrace = CreateEmailTrace(emailMessageDto, trackId);

        await OnSaveEmailTraces(emailTrace, cancellationToken);
    }

    private string OnSendEmail(EmailMessageDto emailMessageDto)
    {
        var smtpClient = CreateSmtpClient();

        var trackId = GenerateUniqueTrackerId();

        var mailMessage = CreateMailMessage(emailMessageDto, trackId);
        
        smtpClient.Send(mailMessage);

        return trackId;
    }

    private async Task OnSaveEmailTraces(EmailTrace emailTrace, CancellationToken cancellationToken)
    {
        await dbContext.EmailTraces.AddAsync(emailTrace, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private SmtpClient CreateSmtpClient()
    {
        return new(_emailConfiguration.Host)
        {
            Port = _emailConfiguration.Port,
            Credentials = new NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password),
            EnableSsl = true
        };
    }

    private MailMessage CreateMailMessage(EmailMessageDto emailMessageDto, string trackerId)
{
    StringBuilder htmlBody = new StringBuilder();

    htmlBody.Append("<html>");
    htmlBody.Append("<head>");
    htmlBody.Append("<style>");
    htmlBody.Append("body { font-family: Arial, sans-serif; margin: 20px; }");
    htmlBody.Append(".header { font-size: 18px; font-weight: bold; color: #333; margin-bottom: 20px; }");
    htmlBody.Append(".content { font-size: 14px; line-height: 1.5; color: #555; }");
    htmlBody.Append(".footer { font-size: 12px; color: #999; margin-top: 30px; }");
    htmlBody.Append("</style>");
    htmlBody.Append("</head>");
    htmlBody.Append("<body>");
    htmlBody.Append($"<div class='header'>{emailMessageDto.Subject}</div>");
    htmlBody.Append("<div class='content'>");
    htmlBody.Append(emailMessageDto.Body);
    htmlBody.Append("</div>");
    htmlBody.Append("<div class='footer'>");
    htmlBody.Append("If you have any questions, please contact us at support@example.com.");
    htmlBody.Append("</div>");
    htmlBody.Append(GenerateTrackingPixel(trackerId));
    htmlBody.Append("</body>");
    htmlBody.Append("</html>");

    var mailMessage = new MailMessage(
        _emailConfiguration.SenderEmail,
        emailMessageDto.To,
        emailMessageDto.Subject,
        htmlBody.ToString()
    )
    {
        IsBodyHtml = true
    };

    return mailMessage;
}

private EmailTrace CreateEmailTrace(EmailMessageDto emailMessageDto, string trackerId)
{
    return EmailTrace.Create(emailMessageDto.To,
        emailMessageDto.Subject,
        emailMessageDto.Body,
        emailMessageDto.MessageId,
        trackerId);
}

private string GenerateTrackingPixel(string trackId)
{
    return $"<img width='1' height='1' src='{_appSettings.BaseUrl}/email/tracking/{trackId}' style='display:none;'>";
}

    private string GenerateUniqueTrackerId() => Guid.NewGuid().ToString();
}