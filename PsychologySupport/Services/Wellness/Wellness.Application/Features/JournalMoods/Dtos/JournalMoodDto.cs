using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.JournalMoods.Dtos
{
    public record JournalMoodDto(
        Guid Id,
        Guid SubjectRef,
        string MoodName,
        string? MoodIconCode,
        string? Note,
        int Value,
        DateTimeOffset CreatedAt
    );
}
