using System;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events.Notification;
using Grpc.Core;
using Notification.API.Outbox.Models;
using Notification.API.Outbox.Services;
using Notification.API.Protos;
using Notification.API.Emails.Events;
using Notification.API.Emails.ServiceContracts;

namespace Notification.API.Services
{
    public class NotificationService : Protos.NotificationService.NotificationServiceBase
    {
        private readonly OutboxService _outboxService;
        private readonly IEmailService _emailService;

        public NotificationService(OutboxService outboxService, IEmailService emailService)
        {
            _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public override async Task<Protos.HasSentEmailRecentlyResponse> HasSentEmailRecently(
            Protos.HasSentEmailRecentlyRequest request,
            ServerCallContext context)
        {
            var isRecentlySent = await _emailService.HasSentEmailRecentlyAsync(
                request.Email,
                context.CancellationToken);

            return new Protos.HasSentEmailRecentlyResponse { IsRecentlySent = isRecentlySent };
        }

        public override async Task<Protos.SendEmailResponse> SendEmail(
            Protos.SendEmailRequest request,
            ServerCallContext context)
        {
            try
            {
                // Nếu client gửi EventId thì parse, nếu không thì tạo mới
                Guid eventId = string.IsNullOrWhiteSpace(request.EventId)
                    ? Guid.NewGuid()
                    : Guid.TryParse(request.EventId, out var parsedId) ? parsedId : throw new FormatException("Invalid EventId format.");

                // 1ạo Domain Event 
                var domainEvent = new SendEmailEvent(
                    eventId,
                    request.To,
                    request.Subject,
                    request.Body
                );

                // Lưu vào Outbox
                var outboxMessage = OutboxMessage.Create(domainEvent);
                await _outboxService.AddAsync(outboxMessage, context.CancellationToken);

                // Gửi email trực tiếp
                var emailMessageDto = new EmailMessageDto(
                    eventId,
                    request.To,
                    request.Subject,
                    request.Body
                );

                await _emailService.SendEmailAsync(emailMessageDto, context.CancellationToken);

                return new SendEmailResponse
                {
                    Success = true,
                    Message = "Email request processed and sent successfully"
                };
            }
            catch (FormatException fmtEx)
            {
                return new SendEmailResponse
                {
                    Success = false,
                    Message = fmtEx.Message
                };
            }
            catch (Exception ex)
            {
                return new SendEmailResponse
                {
                    Success = false,
                    Message = $"Failed to process email request: {ex.Message}"
                };
            }
        }
    }
}
