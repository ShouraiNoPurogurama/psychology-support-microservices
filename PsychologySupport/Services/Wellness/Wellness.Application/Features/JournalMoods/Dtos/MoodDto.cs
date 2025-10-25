using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.JournalMoods.Dtos
{
    public record MoodDto(
         Guid Id,
         string Name,
         string IconCode,
         string? Description
    );
}
