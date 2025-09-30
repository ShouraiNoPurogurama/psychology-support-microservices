using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Events
{
    public static class ModuleSectionDomainEvents
    {
        public record ModuleProgressCreatedEvent(
            Guid ModuleProgressId,
            Guid SubjectRef,
            Guid SectionId,
            IEnumerable<Guid> ArticleIds,
            Guid FirstSectionArticleId
        ) : IDomainEvent;

        public record ModuleProgressUpdatedEvent(
            Guid ModuleProgressId,
            Guid SubjectRef,
            Guid SectionId,
            Guid ArticleId,
            int MinutesRead
        ) : IDomainEvent;
    }
}
