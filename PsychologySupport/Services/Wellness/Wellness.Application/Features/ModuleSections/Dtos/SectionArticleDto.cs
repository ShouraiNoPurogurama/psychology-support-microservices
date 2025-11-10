using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Domain.Aggregates.ModuleSections.ValueObjects;

namespace Wellness.Application.Features.ModuleSections.Dtos
{
    public record SectionArticleDto(
        Guid Id,
        string Title,
        string MediaUrl,
        string Content,
        int OrderIndex,
        int Duration,
        bool Completed,
        ArticleSource ArticleSource,
        bool HasAccess
    );
}
