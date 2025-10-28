using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using ChatBox.API.Data;
using ChatBox.API.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.EventHandlers;

public class RewardGrantedIntegrationEventHandler(
    ChatBoxDbContext dbContext, 
    ILogger<RewardGrantedIntegrationEventHandler> logger) 
    : IConsumer<RewardGrantedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RewardGrantedIntegrationEvent> context)
    {
        var evt = context.Message;
        
        // 1. Kiểm tra Idempotency
        bool alreadyExists = await dbContext.PendingStickerRewards
            .AnyAsync(r => r.RewardId == evt.RewardId, context.CancellationToken);

        if (alreadyExists)
        {
            logger.LogWarning("Sticker reward {RewardId} already processed. Skipping.", evt.RewardId);
            return;
        }

        // 2. Tạo Entity
        var newPendingSticker = new PendingStickerReward(
            evt.RewardId,
            evt.UserId,
            evt.SessionId,
            evt.StickerUrl,
            evt.PromptFilter
        );
        
        // 3. Lưu (thử/bắt lỗi race condition)
        try
        {
            await dbContext.PendingStickerRewards.AddAsync(newPendingSticker, context.CancellationToken);
            await dbContext.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Saved PendingStickerReward {RewardId}", evt.RewardId);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Failed to save {RewardId}, probably race condition.", evt.RewardId);
        }
    }
}