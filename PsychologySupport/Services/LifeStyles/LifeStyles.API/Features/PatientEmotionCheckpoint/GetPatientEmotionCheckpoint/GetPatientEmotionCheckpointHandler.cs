using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos.Emotions;
using LifeStyles.API.Dtos.EmotionSelections;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.GetPatientEmotionCheckpoint;

public record GetPatientEmotionCheckpointQuery(Guid PatientProfileId) : IQuery<GetPatientEmotionCheckpointResult>;

public record GetPatientEmotionCheckpointResult(PatientEmotionCheckpointDto CheckpointDto);

public class GetPatientEmotionCheckpointHandler(LifeStylesDbContext dbContext)
    : IQueryHandler<GetPatientEmotionCheckpointQuery, GetPatientEmotionCheckpointResult>
{
    public async Task<GetPatientEmotionCheckpointResult> Handle(GetPatientEmotionCheckpointQuery request, CancellationToken cancellationToken)
    {
        var checkpoint = await dbContext.PatientEmotionCheckpoints
            .Include(c => c.EmotionSelections)
            .ThenInclude(es => es.Emotion)
            .FirstOrDefaultAsync(c => c.Id == request.PatientProfileId, cancellationToken);

        if (checkpoint == null)
            throw new KeyNotFoundException($"Emotion Checkpoint for Patient ID {request.PatientProfileId} was not found.");

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