using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos.Emotions;
using LifeStyles.API.Dtos.EmotionSelections;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.GetPatientEmotionCheckpoint;

public record GetPatientEmotionCheckpointQuery(Guid PatientProfileId, DateTimeOffset? Date)  : IQuery<GetPatientEmotionCheckpointResult>;

public record GetPatientEmotionCheckpointResult(PatientEmotionCheckpointDto? CheckpointDto);

public class GetPatientEmotionCheckpointHandler(LifeStylesDbContext dbContext)
    : IQueryHandler<GetPatientEmotionCheckpointQuery, GetPatientEmotionCheckpointResult>
{
    public async Task<GetPatientEmotionCheckpointResult> Handle(GetPatientEmotionCheckpointQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date ?? DateTimeOffset.UtcNow;
        
        var checkpoint = await dbContext.PatientEmotionCheckpoints
            .Include(c => c.EmotionSelections)
            .ThenInclude(es => es.Emotion)
            .Where(c => c.PatientProfileId == request.PatientProfileId && c.LogDate.Date <= date.Date)
            .OrderByDescending(c => c.LogDate)
            .FirstOrDefaultAsync(c => c.PatientProfileId == request.PatientProfileId, cancellationToken);

        if (checkpoint == null)
            return new GetPatientEmotionCheckpointResult(null);

        var emotionDtos = checkpoint.EmotionSelections.Select(e => new GetEmotionSelectionDto(
            e.Id,
            new EmotionDto(e.EmotionId, e.Emotion.Name.ToString()),
            e.Intensity,
            e.Rank
        )).ToList();

        var dto = new PatientEmotionCheckpointDto(
            checkpoint.Id,
            emotionDtos,
            checkpoint.LogDate
        );

        return new GetPatientEmotionCheckpointResult(dto);
    }
}