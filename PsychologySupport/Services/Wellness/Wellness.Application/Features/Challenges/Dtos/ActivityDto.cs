using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.Application.Features.Challenges.Dtos
{
    public record ActivityDto(
       Guid Id,
       string Name,
       string? Description,
       ActivityType ActivityType,
       int? Duration,
       string? Instructions
   );
}
