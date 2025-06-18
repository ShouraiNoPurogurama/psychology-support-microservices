using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.CurrentEmotion.GetCurrentEmotion;

public record GetCurrentEmotionQuery(Guid PatientProfileId, DateTime Date)
    : IQuery<GetCurrentEmotionResult>;

public record GetCurrentEmotionResult(Guid Id, Guid PatientProfileId, DateTimeOffset LogDate, string? Emotion1, string? Emotion2);
public class GetCurrentEmotionHandler(LifeStylesDbContext context)
    : IQueryHandler<GetCurrentEmotionQuery, GetCurrentEmotionResult>
{
    public async Task<GetCurrentEmotionResult> Handle(GetCurrentEmotionQuery request, CancellationToken cancellationToken)
    {
        var emotion = await context.CurrentEmotions
            .Where(x =>
                x.PatientProfileId == request.PatientProfileId &&
                x.LogDate.Date <= request.Date.Date)
            .OrderByDescending(x => x.LogDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (emotion is null)
            throw new LifeStylesNotFoundException("CurrentEmotion", request.PatientProfileId);

        return new GetCurrentEmotionResult(
            emotion.Id,
            emotion.PatientProfileId,
            emotion.LogDate,
            emotion.Emotion1?.ToString(),
            emotion.Emotion2?.ToString()
        );
    }
}
