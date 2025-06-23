using BuildingBlocks.CQRS;
using LifeStyles.API.Dtos;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using LifeStyles.API.Features.PatientEmotionCheckpoint.GetPatientEmotionCheckpoint;
using LifeStyles.API.Features.PatientImprovementGoal.GetPatientImprovementGoal;
using MediatR;

namespace LifeStyles.API.Features.DASS21LifestyleInput.BuildDASS21LifestyleInput;

public record BuildDASS21LifestyleInputQuery(
    Guid PatientProfileId,
    DateTime? Date
) : IQuery<BuildDASS21LifestyleInputResult>;

public record BuildDASS21LifestyleInputResult(
    Guid PatientProfileId,
    List<PatientImprovementGoalDto> Goals,
    PatientEmotionCheckpointDto PatientEmotionCheckpoint
);

public class BuildDASS21LifestyleInputHandler(ISender sender) : IQueryHandler<BuildDASS21LifestyleInputQuery, BuildDASS21LifestyleInputResult>
{
    public async Task<BuildDASS21LifestyleInputResult> Handle(BuildDASS21LifestyleInputQuery request, CancellationToken cancellationToken)
    {
        var currentTime = request.Date ?? DateTime.UtcNow;
        
        var goalQuery = new GetPatientImprovementGoalQuery(request.PatientProfileId, currentTime);
        var emotionCheckpointQuery = new GetPatientEmotionCheckpointQuery(request.PatientProfileId, currentTime);
        
        var goals = await sender.Send(goalQuery, cancellationToken);
        var emotionCheckpoint = await sender.Send(emotionCheckpointQuery, cancellationToken);

        var result = new BuildDASS21LifestyleInputResult(
            request.PatientProfileId,
            goals.Goals,
            emotionCheckpoint.CheckpointDto
        );
        
        return result;
    }
}