using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.Application.Features.Challenges.Dtos
{
    public record ChallengeDto(
       Guid Id,
       string Title,
       string? Description,
       ChallengeType ChallengeType,
       int DurationActivity,
       int DurationDate,
       List<ChallengeStepDto> Steps,
       string MediaUrl,
       bool HasAccess
   );
}
