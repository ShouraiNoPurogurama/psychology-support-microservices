using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.ModuleSections.Dtos
{
    public record ModuleSectionDetailsDto(
       Guid Id,
       string Title,
       string MediaUrl,
       string Description,
       int TotalDuration,
       int CompletedDuration,
       bool Completed,
       int ArticleCount,
       List<SectionArticleDto> Articles
   );
}
