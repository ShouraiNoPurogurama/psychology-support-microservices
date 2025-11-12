using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Data.Options;
using UserMemory.API.Models;
using UserMemory.API.Shared.Authentication;
using UserMemory.API.Shared.Outbox;
using UserMemory.API.Shared.Services.Contracts; 
using UserMemory.API.Shared.Enums; 

namespace UserMemory.API.Domains.Rewards.Features.ClaimReward;

public record ClaimRewardCommand(Guid IdempotencyKey, Guid ChatSessionId) : IdempotentCommand<ClaimRewardResult>(IdempotencyKey);

public record ClaimRewardResult(Guid RewardId, string StickerGenerationJobStatus);

/// <summary>
/// Xử lý logic đổi thưởng (Claim Reward).
/// Business Rule:
/// 1. (Cô lập Session): Phải dùng 1000 điểm kiếm được TỪ CHÍNH session đang chat.
/// 2. (Giới hạn Free): User free chỉ được đổi 1 lần/ngày (tổng cộng, bất kể session nào).
/// </summary>
    public class ClaimRewardHandler(
    UserMemoryDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor,
    IOutboxWriter outboxWriter,
    ICurrentUserSubscriptionAccessor subAccessor 
) : ICommandHandler<ClaimRewardCommand, ClaimRewardResult>
{
    public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
    {
        var rewardCost = StickerGenerationQuotaOptions.REWARD_COST;
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var aliasId = currentActorAccessor.GetRequiredAliasId();
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var userTier = subAccessor.GetCurrentTier(); 
            var maxClaimsForThisUser = StickerGenerationQuotaOptions.GetMaxClaimsForTier(userTier);

            AliasDailySummary? summary = null; 

            
            summary = await dbContext.AliasDailySummaries
                .FirstOrDefaultAsync(s => s.AliasId == aliasId && s.Date == currentDate, cancellationToken);

            if (summary is null)
            {
                // "Seed" (Tạo) record tóm tắt nếu là lần đầu trong ngày
                summary = new AliasDailySummary
                {
                    AliasId = aliasId,
                    Date = currentDate,
                    RewardClaimCount = 0 
                };
                dbContext.AliasDailySummaries.Add(summary);
            }

            // Check giới hạn DỰA TRÊN GIỚI HẠN ĐỘNG (dynamic)
            if (summary.RewardClaimCount >= maxClaimsForThisUser)
            {
                throw new BadRequestException("Bạn đã hết lượt đổi thưởng hôm nay cho gói đăng ký của mình.");
            }

            var rowsAffected = await dbContext.SessionDailyProgresses
                .Where(p => p.AliasId == aliasId &&
                            p.SessionId == request.ChatSessionId && 
                            p.ProgressPoints >= rewardCost) 
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.ProgressPoints, p => 0),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new BadRequestException("Session chat này không đủ 1000 điểm để đổi.");
            }

            summary.RewardClaimCount++; 
            summary.LastModified = DateTimeOffset.UtcNow;
            

            var newReward = new Reward
            {
                Id = Guid.NewGuid(),
                AliasId = aliasId,
                SessionId = request.ChatSessionId,
                PointsCost = rewardCost,
                Status = RewardStatus.Pending 
            };
            dbContext.Rewards.Add(newReward);

            var rewardEvent = new RewardRequestedIntegrationEvent(newReward.Id, aliasId, request.ChatSessionId);
            await outboxWriter.WriteAsync(rewardEvent, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new ClaimRewardResult(newReward.Id, "Queued");
        }
        catch (Exception)
        {
            throw; 
        }
    }
}