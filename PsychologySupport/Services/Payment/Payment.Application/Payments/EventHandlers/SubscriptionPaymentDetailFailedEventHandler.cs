using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class SubscriptionPaymentDetailFailedEventHandler(
    IPublishEndpoint publishEndpoint,
    IRequestClient<GetUserDataRequest> authClient,
    ILogger<SubscriptionPaymentDetailFailedEventHandler> logger)
    : INotificationHandler<SubscriptionPaymentDetailFailedEvent>
{
    public async Task Handle(SubscriptionPaymentDetailFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Domain Event handled: {DomainEvent}", notification.GetType());
        
        SubscriptionPaymentFailedIntegrationEvent paymentFailedEvent =
            notification.Adapt<SubscriptionPaymentFailedIntegrationEvent>();

        // Load HTML template
        var basePath = Path.Combine(AppContext.BaseDirectory, "Payments", "EmailTemplates");
        var templatePath = Path.Combine(basePath, "SubscriptionPaymentFailed.html");

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var body = template.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        // Gửi email 
        var sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            notification.PatientEmail,
            "Thanh toán gói đăng ký thất bại",
            body);

        var userDataResponse =
            await authClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(null, notification.PatientEmail),
                cancellationToken);

        var FCMTokens = userDataResponse.Message.FCMTokens;

        if (FCMTokens.Any())
        {
            var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
                FCMTokens,     "Thanh toán gói đăng ký thất bại",
                "Thanh toán cho gói đăng ký của bạn đã thất bại. Vui lòng kiểm tra lại thông tin thanh toán và thử lại.");

            await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
        }
        
        await publishEndpoint.Publish(paymentFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}