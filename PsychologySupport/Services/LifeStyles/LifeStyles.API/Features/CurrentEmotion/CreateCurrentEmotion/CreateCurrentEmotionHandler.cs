using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using LifeStyles.API.Data;
using LifeStyles.API.Models;

namespace LifeStyles.API.Features.CurrentEmotion.CreateCurrentEmotion;

public record CreateCurrentEmotionCommand(
    Guid PatientProfileId,
    Emotion? Emotion1,
    Emotion? Emotion2
) : ICommand<CreateCurrentEmotionResult>;

public record CreateCurrentEmotionResult(Guid Id);
public class CreateCurrentEmotionHandler(LifeStylesDbContext context)
    : ICommandHandler<CreateCurrentEmotionCommand, CreateCurrentEmotionResult>
{
    public async Task<CreateCurrentEmotionResult> Handle(CreateCurrentEmotionCommand request, CancellationToken cancellationToken)
    {
        var emotion = new Models.CurrentEmotion
        {
            Id = Guid.NewGuid(),
            PatientProfileId = request.PatientProfileId,
            Emotion1 = request.Emotion1,
            Emotion2 = request.Emotion2,
            LogDate = DateTimeOffset.UtcNow
        };

        context.CurrentEmotions.Add(emotion);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateCurrentEmotionResult(emotion.Id);
    }
}
