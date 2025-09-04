using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class UpgradeSubscriptionPaymentDetailFailedEventHandler(
    IPublishEndpoint publishEndpoint,
    IRequestClient<GetUserDataRequest> authClient,
    ILogger<UpgradeSubscriptionPaymentDetailFailedEventHandler> logger)
    : INotificationHandler<UpgradeSubscriptionPaymentDetailFailedEvent>
{
    public async Task Handle(UpgradeSubscriptionPaymentDetailFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Domain Event handled: {DomainEvent}", notification.GetType());

        UpgradeSubscriptionPaymentFailedIntegrationEvent paymentDetailDetailFailedEvent =
            notification.Adapt<UpgradeSubscriptionPaymentFailedIntegrationEvent>();

        SendEmailIntegrationEvent sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            notification.PatientEmail,
            "Thanh toán gói đăng ký thất bại",
            "Thanh toán cho gói đăng ký của bạn đã thất bại. Vui lòng kiểm tra lại thông tin thanh toán và thử lại.");

        var userDataResponse =
            await authClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(null, notification.PatientEmail),
                cancellationToken);

        var FCMTokens = userDataResponse.Message.FCMTokens;

        if (FCMTokens.Any())
        {
            var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
                FCMTokens,      "Thanh toán gói đăng ký thất bại",
                "Thanh toán cho gói đăng ký của bạn đã thất bại. Vui lòng kiểm tra lại thông tin thanh toán và thử lại.");

            await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
        }

        await publishEndpoint.Publish(paymentDetailDetailFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}