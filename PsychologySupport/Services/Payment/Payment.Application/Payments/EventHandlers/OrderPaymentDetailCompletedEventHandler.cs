using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers
{
    public class OrderPaymentDetailCompletedEventHandler(
        ILogger<OrderPaymentDetailCompletedEventHandler> logger,
        IPublishEndpoint publishEndpoint,
        IRequestClient<GetUserDataRequest> authClient)
        : INotificationHandler<OrderPaymentDetailCompletedEvent>
    {
        public async Task Handle(OrderPaymentDetailCompletedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("*** Handling OrderPaymentDetailCompletedEvent for OrderId: {OrderId}",
                notification.OrderId);

            // Event để kích hoạt đơn hàng
            //var activateOrderEvent = new OrderPaymentSuccessIntegrationEvent(notification.OrderId);

            // Event gửi mail xác nhận
            //var sendEmailEvent = new SendEmailIntegrationEvent(
            //    notification.CustomerEmail,
            //    "Thanh toán đơn hàng thành công",
            //    $"Đơn hàng {notification.OrderId} của bạn đã được thanh toán thành công."
            //);

            // Lấy thông tin user để push notification
            //var userDataResponse = await authClient.GetResponse<GetUserDataResponse>(
            //    new GetUserDataRequest(null, notification.CustomerEmail),
            //    cancellationToken);

            //var FCMTokens = userDataResponse.Message.FCMTokens;

            //if (FCMTokens.Any())
            //{
            //    var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
            //        FCMTokens,
            //        "Thanh toán đơn hàng thành công",
            //        $"Đơn hàng {notification.OrderId} của bạn đã được thanh toán thành công."
            //    );

            //    await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
            //}

            //await publishEndpoint.Publish(activateOrderEvent, cancellationToken);
            //await publishEndpoint.Publish(sendEmailEvent, cancellationToken);
        }
    }
}
