using Microsoft.EntityFrameworkCore;
using Wellness.Domain.Models;

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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
