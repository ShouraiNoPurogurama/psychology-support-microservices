using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Gifts.DomainEvents;

namespace Post.Application.Features.Gifts.EventHandlers;

public class GiftAttachedEventHandler(ILogger<GiftAttachedEventHandler> logger, 
    IQueryDbContext queryDbContext,
    IOutboxWriter outboxWriter)
    : INotificationHandler<GiftAttachedEvent>
{
    public async Task Handle(GiftAttachedEvent notification, CancellationToken cancellationToken)
    {
        //TODO Tạm thời làm theo kiểu anti pattern, khúc này đáng ra bên consumer phải tự enrich label vào event, nhưng giờ ko có thời gian nên làm tạm vậy
        var aliasLabel = await queryDbContext.AliasVersionReplica
            .Where(a => a.AliasId == notification.SenderAliasId)
            .Select(a => a.Label)
            .FirstAsync(cancellationToken);
        
        await outboxWriter.WriteAsync(new GiftAttachedIntegrationEvent(
                PostId: notification.PostId,
                GiftId: notification.GiftId,
                SenderAliasLabel: aliasLabel,
                SenderAliasId: notification.SenderAliasId,
                PostAuthorAliasId: notification.PostAuthorAliasId,
                Amount: notification.Amount,
                Message: notification.Message,
                SentAt: notification.SentAt
            ), cancellationToken
        );
    }
}