using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Data.Options;
using UserMemory.API.Shared.Authentication;
using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Domains.Progress.Features.GetCurrentSessionRewardProgress;

public record GetCurrentSessionRewardProgressQuery(Guid SessionId) : IQuery<GetCurrentSessionRewardProgressResult>;

public record GetCurrentSessionRewardProgressResult(
    Guid SessionId,
    int ProgressPoint,
    int LastIncrement,
    DateOnly ProgressDate,
    int ClaimedToday,    // số lượt đã claim hôm nay
    int DailyLimit,      // limit cứng (vd 3)
    int RemainingToday,  // = max(0, DailyLimit - ClaimedToday)
    bool CanClaim        // RemainingToday > 0 && ProgressPoint >= threshold
);

public class GetCurrentSessionRewardProgressHandler(
    UserMemoryDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor)
    : IQueryHandler<GetCurrentSessionRewardProgressQuery, GetCurrentSessionRewardProgressResult>
{
    private  int DAILY_LIMIT = StickerGenerationQuotaOptions.MAX_FREE_CLAIMS_PER_DAY;
    private  int PROGRESS_THRESHOLD = StickerGenerationQuotaOptions.REWARD_COST; 

    public async Task<GetCurrentSessionRewardProgressResult> Handle(
        GetCurrentSessionRewardProgressQuery request,
        CancellationToken cancellationToken)
    {
        var aliasId = currentActorAccessor.GetRequiredAliasId();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // 1) Lấy progress mới nhất của session
        var progressRow = await dbContext.SessionDailyProgresses
            .AsNoTracking()
            .Where(rp => rp.AliasId == aliasId && rp.SessionId == request.SessionId)
            .OrderByDescending(rp => rp.ProgressDate)
            .Select(rp => new
            {
                rp.SessionId,
                rp.ProgressPoints,
                rp.LastIncrement,
                rp.ProgressDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (progressRow is null)
            throw new NotFoundException("Không tìm thấy tiến trình phần thưởng cho phiên chat hiện tại.");

        // 2) ClaimedToday: ưu tiên bảng tổng hợp theo ngày
        var claimedToday = await dbContext.AliasDailySummaries
            .AsNoTracking()
            .Where(x => x.AliasId == aliasId && x.Date == today)
            .Select(x => x.RewardClaimCount)
            .FirstOrDefaultAsync(cancellationToken);

        // 2b) Fallback: nếu chưa có dòng tổng hợp hôm nay thì đếm từ rewards
        if (claimedToday == 0)
        {
            var claimedStatuses = Enum.GetValues<RewardStatus>();
            
            var todayUtc = DateTime.UtcNow.Date;         
            var tomorrowUtc = todayUtc.AddDays(1);        

            claimedToday = await dbContext.Rewards
                .AsNoTracking()
                .Where(r => r.AliasId == aliasId
                            && claimedStatuses.Contains(r.Status)  
                            && r.CreatedAt >= todayUtc               
                            && r.CreatedAt <  tomorrowUtc)
                .CountAsync(cancellationToken);
        }

        var remaining = Math.Max(0, DAILY_LIMIT - claimedToday);
        var canClaim = progressRow.ProgressPoints >= PROGRESS_THRESHOLD && remaining > 0;

        return new GetCurrentSessionRewardProgressResult(
            progressRow.SessionId,
            progressRow.ProgressPoints,
            progressRow.LastIncrement,
            progressRow.ProgressDate,
            ClaimedToday: claimedToday,
            DailyLimit: DAILY_LIMIT,
            RemainingToday: remaining,
            CanClaim: canClaim
        );
    }
}
