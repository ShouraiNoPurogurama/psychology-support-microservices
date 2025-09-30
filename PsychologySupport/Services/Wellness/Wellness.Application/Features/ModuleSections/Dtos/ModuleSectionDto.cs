using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.ModuleSections.Dtos
{
    public record ModuleSectionDto(
        Guid Id,
        string Title,
        string MeidaUrl,
        string Description,
        int TotalDuration,
        int? CompletedDuration,
        ProcessStatus ProcessStatus,
        int ArticleCount
    );
}
