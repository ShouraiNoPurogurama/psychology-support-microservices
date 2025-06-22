using BuildingBlocks.CQRS;
using FluentValidation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos.Emotions;
using LifeStyles.API.Dtos.EmotionSelections;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.CreatePatientEmotionCheckpoint;

public record CreatePatientEmotionCheckpointCommand(
    Guid PatientProfileId,
    List<CreateEmotionSelectionDto> Emotions,
    DateTimeOffset LogDate
) : ICommand<CreatePatientEmotionCheckpointResult>;

public record CreatePatientEmotionCheckpointResult(
    PatientEmotionCheckpointDto CheckpointDto
);

public class CreatePatientEmotionCheckpointValidator 
    : AbstractValidator<CreatePatientEmotionCheckpointCommand>
{
    public CreatePatientEmotionCheckpointValidator()
    {
        RuleFor(x => x.Emotions)
            .NotEmpty()
            .WithMessage("At least one emotion must be provided.");

        RuleFor(x => x.Emotions)
            .Must(emotions => emotions.GroupBy(e => e.EmotionId).All(g => g.Count() == 1))
            .WithMessage("Duplicate emotion(s) detected.");

        RuleFor(x => x.Emotions)
            .Must(emotions =>
                emotions
                    .Where(e => e.Rank.HasValue)
                    .GroupBy(e => e.Rank)
                    .All(g => g.Count() == 1))
            .WithMessage("Duplicate ranks found. Each emotion must have a unique rank.");
        
        RuleFor(x => x.LogDate)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Log date cannot be in the future.");
    }
}


public class CreatePatientEmotionCheckpointHandler(LifeStylesDbContext dbContext)
    : ICommandHandler<CreatePatientEmotionCheckpointCommand,
        CreatePatientEmotionCheckpointResult>
{
    public async Task<CreatePatientEmotionCheckpointResult> Handle(CreatePatientEmotionCheckpointCommand request,
        CancellationToken cancellationToken)
    {
        var emotionIds = request.Emotions.Select(e => e.EmotionId).ToList();

        var existingEmotionIds = await dbContext.Emotions
            .Where(e => emotionIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var invalidIds = emotionIds.Except(existingEmotionIds).ToList();
        if (invalidIds.Count != 0)
            throw new ValidationException($"Invalid EmotionId(s): {string.Join(", ", invalidIds)}");

        
        var checkpoint = new Models.PatientEmotionCheckpoint
        {
            Id = Guid.NewGuid(),
            PatientProfileId = request.PatientProfileId,
            LogDate = request.LogDate
        };

        var emotionSelections = request.Emotions.Select(e => new Models.EmotionSelection
            {
                Id = Guid.NewGuid(),
                EmotionId = e.EmotionId,
                Intensity = e.Intensity,
                Rank = e.Rank,
            })
            .ToList();

        checkpoint.EmotionSelections = emotionSelections;

        dbContext.PatientEmotionCheckpoints.Add(checkpoint);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var emotionNames = await dbContext.Emotions
            .Where(e => emotionSelections.Select(x => x.EmotionId).Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var emotionDtos = emotionSelections.Select(e => new GetEmotionSelectionDto(
                e.Id,
                new EmotionDto(e.EmotionId, emotionNames[e.EmotionId].Name.ToString()),
                e.Intensity,
                e.Rank
            ))
            .ToList();

        return new CreatePatientEmotionCheckpointResult(
            new PatientEmotionCheckpointDto(
                checkpoint.Id,
                emotionDtos,
                checkpoint.LogDate
            )
        );
    }
}