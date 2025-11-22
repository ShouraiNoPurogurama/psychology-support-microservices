using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Data.Options;
using UserMemory.API.Shared.Authentication;

namespace UserMemory.API.Domains.Progress.Features.GetCurrentSessionRewardProgress;

public record GetCurrentSessionRewardProgressQuery(Guid SessionId) : IQuery<GetCurrentSessionRewardProgressResult>;

public record GetCurrentSessionRewardProgressResult(
    Guid SessionId,
    int ProgressPoint,
    int LastIncrement,
    int RewardCost,
    DateOnly ProgressDate
);

public class GetCurrentSessionRewardProgressHandler(
    UserMemoryDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor)
    : IQueryHandler<GetCurrentSessionRewardProgressQuery, GetCurrentSessionRewardProgressResult>
{

    public async Task<GetCurrentSessionRewardProgressResult> Handle(
        GetCurrentSessionRewardProgressQuery request,
        CancellationToken cancellationToken)
    {
        var aliasId = currentActorAccessor.GetRequiredAliasId();
        
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
        {
            return new GetCurrentSessionRewardProgressResult(
                request.SessionId,
                0,
                0,
                StickerGenerationQuotaOptions.REWARD_COST,
                DateOnly.FromDateTime(DateTime.Now)
            );
        }
        
        return new GetCurrentSessionRewardProgressResult(
            progressRow.SessionId,
            progressRow.ProgressPoints,
            progressRow.LastIncrement,
            StickerGenerationQuotaOptions.REWARD_COST,
            progressRow.ProgressDate
        );
    }
}