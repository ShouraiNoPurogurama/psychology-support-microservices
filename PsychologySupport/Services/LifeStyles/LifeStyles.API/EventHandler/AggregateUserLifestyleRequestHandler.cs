using BuildingBlocks.Messaging.Dtos.LifeStyles;
using BuildingBlocks.Messaging.Events.LifeStyle;
using LifeStyles.API.Features.DASS21LifestyleInput.BuildDASS21LifestyleInput;
using MassTransit;
using MediatR;

namespace LifeStyles.API.EventHandler;

public class AggregateUserLifestyleRequestHandler(ISender sender) : IConsumer<AggregatePatientLifestyleRequest>
{
    public async Task Consume(ConsumeContext<AggregatePatientLifestyleRequest> context)
    {
        var query = new BuildDASS21LifestyleInputQuery(context.Message.ProfileId, context.Message.Date);
        var result = await sender.Send(query, context.CancellationToken);

        var response = MapToAggregateUserLifestyleResponse(result);

        await context.RespondAsync(response);
    }

    private static AggregatePatientLifestyleResponse MapToAggregateUserLifestyleResponse(BuildDASS21LifestyleInputResult result)
    {
        List<PatientImprovementGoalFlatDto> goalsResponse = [];

        if (result.Goals.Count > 0)
        {
            goalsResponse = result.Goals.Select(g => new PatientImprovementGoalFlatDto(
                g.GoalId, g.GoalName, g.AssignedAt)).ToList();
        }

        var logDate = result.PatientEmotionCheckpoint.LogDate;

        var emotionResponse = result.PatientEmotionCheckpoint.EmotionSelections.Select(e =>
            new EmotionSelectionFlatDto(
                e.Id,
                e.Emotion.Id,
                e.Emotion.Name,
                e.Intensity,
                e.Rank,
                logDate
            )).ToList();

        return new AggregatePatientLifestyleResponse(
            result.PatientProfileId,
            goalsResponse,
            emotionResponse
        );
    }
}
