using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BuildingBlocks.Constants;
using Microsoft.Extensions.Options;
using Notification.API.Features.Emails.Contracts;
using Notification.API.Features.Emails.Models;
using Notification.API.Infrastructure.Data;
using Notification.API.Infrastructure.Data.Configurations;

namespace Notification.API.Features.Emails.Services;

public class EmailService(
    IOptions<AppSettings> appsettingsOptions,
    NotificationDbContext dbContext
) : IEmailService
{
    private readonly AppSettings _appSettings = appsettingsOptions.Value;
    private readonly EmailConfiguration _emailConfiguration = appsettingsOptions.Value.Features.Email;

    public async Task<bool> HasSentEmailRecentlyAsync(string email, CancellationToken cancellationToken)
    {
        var recentEmail = await dbContext.EmailTraces
            .Where(e => e.To == email && e.CreatedAt > DateTimeOffset.UtcNow.AddHours(7).AddMinutes(-1))
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return recentEmail is not null;
    }

    public async Task SendEmailAsync(EmailMessageDto emailMessageDto, CancellationToken cancellationToken)
    {
        var trackId = await OnSendEmail(emailMessageDto);

        var emailTrace = CreateEmailTrace(emailMessageDto, trackId);

        await OnSaveEmailTraces(emailTrace, cancellationToken);
    }

    private async Task<string> OnSendEmail(EmailMessageDto emailMessageDto)
    {
        if (!Regex.IsMatch(emailMessageDto.To, MyPatterns.Email))
        {
            return "[ERR]" + GenerateUniqueTrackerId();
            ;
        }

        var smtpClient = CreateSmtpClient();

        var trackId = GenerateUniqueTrackerId();

        var mailMessage = CreateMailMessage(emailMessageDto, trackId);

        await smtpClient.SendMailAsync(mailMessage);

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
        var messageId = GenerateDomainMessageId("emoease.vn");

        StringBuilder htmlBody = new StringBuilder();

        htmlBody.Append(emailMessageDto.Body);

        var mailMessage = new MailMessage(
            _emailConfiguration.SenderEmail,
            emailMessageDto.To,
            emailMessageDto.Subject,
            htmlBody.ToString()
        )
        {
            IsBodyHtml = true,
            From = new MailAddress(_emailConfiguration.SenderEmail, "EmoEase")
        };

        mailMessage.Headers.Remove("Message-ID");
        mailMessage.Headers.Add("Message-ID", $"<{messageId}>");

        return mailMessage;
    }
    
    //Giúp tạo Message-ID chuẩn RFC5322: <random.timestamp@domain>
    private static string GenerateDomainMessageId(string domain)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var rnd = Guid.NewGuid().ToString("N").Substring(0, 16);
        return $"{rnd}.{ts}@{domain}";
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