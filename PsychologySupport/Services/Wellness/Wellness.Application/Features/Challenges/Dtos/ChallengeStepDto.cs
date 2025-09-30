using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.Challenges.Dtos
{
    public record ChallengeStepDto(
        Guid Id,
        Guid? ChallengeId,
        Guid? ActivityId,
        int DayNumber,
        int OrderIndex,
        ActivityDto? Activity
    );
}
