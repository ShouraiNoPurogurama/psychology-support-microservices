using Microsoft.EntityFrameworkCore;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.IdempotencyKey;
using Wellness.Domain.Aggregates.JournalMoods;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Aggregates.OutboxMessage;
using Wellness.Domain.Aggregates.ProcessHistories;

namespace Wellness.Application.Data
{
    public interface IWellnessDbContext
    {
        DbSet<Activity> Activities { get; }
        DbSet<ArticleProgress> ArticleProgresses { get; }
        DbSet<Challenge> Challenges { get; }
        DbSet<ChallengeProgress> ChallengeProgresses { get; }
        DbSet<ChallengeStep> ChallengeSteps { get; }
        DbSet<ChallengeStepProgress> ChallengeStepProgresses { get; }
        DbSet<IdempotencyKey> IdempotencyKeys { get; }
        DbSet<JournalMood> JournalMoods { get; }
        DbSet<ModuleProgress> ModuleProgresses { get; }
        DbSet<ModuleSection> ModuleSections { get; }
        DbSet<OutboxMessage> OutboxMessages { get; }
        DbSet<ProcessHistory> ProcessHistories { get; }
        DbSet<SectionArticle> SectionArticles { get; }
        DbSet<WellnessModule> WellnessModules { get; }
        DbSet<Mood> Moods{ get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
