using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.ProcessHistories.Dtos;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.ProcessHistories.Queries
{
    public record GetUserProgressStatsQuery(Guid SubjectRef) : IQuery<GetUserProgressStatsResult>;

    public record GetUserProgressStatsResult(UserProgressStatsDto Stats);

    public class GetUserProgressStatsHandler : IQueryHandler<GetUserProgressStatsQuery, GetUserProgressStatsResult>
    {
        private readonly IWellnessDbContext _context;

        public GetUserProgressStatsHandler(IWellnessDbContext context)
        {
            _context = context;
        }

        public async Task<GetUserProgressStatsResult> Handle(GetUserProgressStatsQuery request, CancellationToken cancellationToken)
        {
            var subjectRef = request.SubjectRef;

            // 1️⃣ Articles: tổng số bài đã đọc & tổng thời gian đọc
            var articleQuery = from ap in _context.ArticleProgresses
                               join sa in _context.SectionArticles on ap.ArticleId equals sa.Id
                               where ap.ModuleProgress!.SubjectRef == subjectRef &&
                                     ap.ProcessStatus == ProcessStatus.Completed
                               select sa.Duration;

            int totalArticlesRead = await articleQuery.CountAsync(cancellationToken);
            int totalReadingMinutes = totalArticlesRead > 0
                ? await articleQuery.SumAsync(cancellationToken)
                : 0;

            // 2️⃣ Challenges: tổng số challenge hoàn thành
            int totalChallengesCompleted = await _context.ChallengeProgresses
                .AsNoTracking()
                .CountAsync(cp => cp.SubjectRef == subjectRef &&
                                  cp.ProcessStatus == ProcessStatus.Completed, cancellationToken);

            // 3️⃣ Activity: tổng thời gian hoàn thành cho từng loại hoạt động
            var activityDurations = await _context.ProcessHistories
                .AsNoTracking()
                .Where(ph => ph.SubjectRef == subjectRef && ph.ProcessStatus == ProcessStatus.Completed)
                .Include(ph => ph.Activity)
                .GroupBy(ph => ph.Activity!.ActivityType)
                .Select(g => new
                {
                    ActivityType = g.Key.ToString(),
                    TotalDuration = g.Sum(x => x.Activity!.Duration ?? 0)
                })
                .ToDictionaryAsync(x => x.ActivityType, x => x.TotalDuration, cancellationToken);

            var dto = new UserProgressStatsDto(
                TotalArticlesRead: totalArticlesRead,
                TotalReadingMinutes: totalReadingMinutes,
                TotalChallengesCompleted: totalChallengesCompleted,
                ActivityDurations: activityDurations
            );

            return new GetUserProgressStatsResult(dto);
        }
    }
}
