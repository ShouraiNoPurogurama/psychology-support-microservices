using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.ProcessHistories.Dtos
{
    public record UserProgressStatsDto(
        int TotalArticlesRead,                     // Tổng số bài đã đọc
        int TotalReadingMinutes,                   // Tổng thời gian đọc (phút)
        int TotalChallengesCompleted,              // Tổng số Challenge hoàn thành
        IDictionary<string, int> ActivityDurations // Tổng thời gian hoàn thành cho từng ActivityType (phút)
    );
}
