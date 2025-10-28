using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
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
    private const int REWARD_COST = 1000;
    private const int MAX_FREE_CLAIMS_PER_DAY = 1;

    public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
    {
        // Bắt đầu 1 transaction bao gồm tất cả các thao tác DB
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var aliasId = currentActorAccessor.GetRequiredAliasId();
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var isFreeUser = subAccessor.IsFreeTier();

            AliasDailySummary? summary = null; 

            // --- KIỂM TRA ĐIỀU KIỆN 2: GIỚI HẠN FREE USER ---
            if (isFreeUser)
            {
                // Tìm record tóm tắt của ngày
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

                // Check giới hạn
                if (summary.RewardClaimCount >= MAX_FREE_CLAIMS_PER_DAY)
                {
                    // Đã hết lượt, ném lỗi -> transaction tự động rollback
                    throw new BadRequestException("Bạn đã hết lượt đổi thưởng free hôm nay.");
                }
            }

            // --- KIỂM TRA ĐIỀU KIỆN 1: ĐIỂM CỦA SESSION ---
            // Dùng Atomic Update (ExecuteUpdateAsync) để trừ điểm
            var rowsAffected = await dbContext.SessionDailyProgresses
                .Where(p => p.AliasId == aliasId &&
                            p.SessionId == request.ChatSessionId && // Check đúng session
                            p.ProgressDate == currentDate &&
                            p.ProgressPoints >= REWARD_COST) // Check đủ điểm trong session
                .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.ProgressPoints, p => p.ProgressPoints - REWARD_COST), // Trừ điểm của session
                    cancellationToken);

            // Kiểm tra xem có trừ điểm được không
            if (rowsAffected == 0)
            {
                // Lý do: Session này không đủ 1000 điểm
                // Không cần RollbackAsync vì chưa commit, `await using` sẽ lo
                throw new BadRequestException("Session chat này không đủ 1000 điểm để đổi.");
            }

            // --- CẬP NHẬT BỘ ĐẾM (NẾU LÀ FREE) ---
            if (isFreeUser && summary != null)
            {
                summary.RewardClaimCount++; 
                summary.LastModified = DateTimeOffset.UtcNow;
            }

            // --- TẠO RECORD PHẦN THƯỞNG VÀ EVENT ---

            // 1. Tạo Reward entity
            var newReward = new Reward
            {
                Id = Guid.NewGuid(),
                AliasId = aliasId,
                SessionId = request.ChatSessionId,
                PointsCost = REWARD_COST,
                Status = RewardStatus.Pending // Dùng RewardStatus.Pending
            };
            dbContext.Rewards.Add(newReward);

            // 2. Tạo Integration Event để ghi vào Outbox
            var rewardEvent = new RewardRequestedIntegrationEvent(newReward.Id, aliasId, request.ChatSessionId);
            await outboxWriter.WriteAsync(rewardEvent, cancellationToken);
            
            // 3. Lưu tất cả thay đổi vào DbContext
            // (Bao gồm: Thêm Reward + Thêm OutboxMessage + Cập nhật/Thêm AliasDailySummary)
            await dbContext.SaveChangesAsync(cancellationToken);

            // 4. COMMIT TRANSACTION
            // Nếu tất cả thành công, commit các thay đổi
            await transaction.CommitAsync(cancellationToken);

            // 5. Trả về kết quả
            return new ClaimRewardResult(newReward.Id, "Queued");
        }
        catch (Exception)
        {
            // Nếu có bất kỳ lỗi nào ở trên, `await using` sẽ tự động
            // rollback transaction. Ném lại lỗi để middleware xử lý.
            throw; 
        }
    }
}