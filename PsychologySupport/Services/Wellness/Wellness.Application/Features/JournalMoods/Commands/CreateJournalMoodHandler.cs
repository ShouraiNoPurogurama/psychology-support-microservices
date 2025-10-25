using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.JournalMoods;

namespace Wellness.Application.Features.JournalMoods.Commands
{
    public record CreateJournalMoodCommand(
        Guid IdempotencyKey,
        Guid SubjectRef,
        Guid MoodId,
        string? Note
    ) : IdempotentCommand<CreateJournalMoodResult>(IdempotencyKey);

    public record CreateJournalMoodResult(
        Guid JournalMoodId,
        Guid SubjectRef,
        Guid MoodId,
        string? Note
    );

    internal class CreateJournalMoodHandler
        : ICommandHandler<CreateJournalMoodCommand, CreateJournalMoodResult>
    {
        private readonly IWellnessDbContext _dbContext;

        public CreateJournalMoodHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateJournalMoodResult> Handle(
            CreateJournalMoodCommand request,
            CancellationToken cancellationToken)
        {
            // Kiểm tra trùng IdempotencyKey

            var moodExists = await _dbContext.Moods
                .AnyAsync(x => x.Id == request.MoodId, cancellationToken);

            if (!moodExists)
                throw new NotFoundException(nameof(Mood), request.MoodId);

            var journalMood = JournalMood.Create(
                subjectRef: request.SubjectRef,
                moodId: request.MoodId,
                note: request.Note
            );

            _dbContext.JournalMoods.Add(journalMood);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateJournalMoodResult(
                journalMood.Id,
                journalMood.SubjectRef,
                journalMood.MoodId,
                journalMood.Note
            );
        }
    }
}
