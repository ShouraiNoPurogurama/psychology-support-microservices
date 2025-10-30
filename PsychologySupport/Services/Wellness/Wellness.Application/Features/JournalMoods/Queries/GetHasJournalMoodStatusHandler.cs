using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.JournalMoods.Queries;
using System;

namespace Wellness.Application.Features.JournalMoods.Queries
{
    public record GetHasJournalMoodStatusQuery(
        Guid SubjectRef
    ) : IQuery<GetHasJournalMoodStatusResult>;

    public record GetHasJournalMoodStatusResult(bool HasMood);

    internal class GetHasJournalMoodStatusHandler
        : IQueryHandler<GetHasJournalMoodStatusQuery, GetHasJournalMoodStatusResult>
    {
        private readonly IWellnessDbContext _context;

        public GetHasJournalMoodStatusHandler(IWellnessDbContext context)
        {
            _context = context;
        }

        public async Task<GetHasJournalMoodStatusResult> Handle(
            GetHasJournalMoodStatusQuery request,
            CancellationToken cancellationToken)
        {
            // Chọn timezone Việt Nam
            TimeZoneInfo vietnamTimeZone;
            try
            {
                // Windows
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                // Linux/Mac
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }

            // Lấy giờ hiện tại theo timezone VN
            var nowVn = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTimeZone);

            // Xác định đầu và cuối ngày hôm nay theo VN
            var startOfTodayVn = nowVn.Date;
            var endOfTodayVn = startOfTodayVn.AddDays(1).AddTicks(-1);

            // Chuyển sang UTC để so sánh với DB
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfTodayVn, vietnamTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endOfTodayVn, vietnamTimeZone);

            // Query kiểm tra xem SubjectRef đã có mood hôm nay chưa
            var hasMood = await _context.JournalMoods
                .AsNoTracking()
                .AnyAsync(jm => jm.SubjectRef == request.SubjectRef &&
                                jm.CreatedAt >= startUtc &&
                                jm.CreatedAt <= endUtc,
                          cancellationToken);

            return new GetHasJournalMoodStatusResult(hasMood);
        }
    }
}
