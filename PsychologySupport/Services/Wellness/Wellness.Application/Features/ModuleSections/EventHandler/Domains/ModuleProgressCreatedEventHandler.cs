using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;
using static Wellness.Domain.Events.ModuleSectionDomainEvents;

namespace Wellness.Application.Features.ModuleSections.EventHandler.Domains
{
    internal class ModuleProgressCreatedEventHandler
        : INotificationHandler<ModuleProgressCreatedEvent>
    {
        private readonly IWellnessDbContext _dbContext;

        public ModuleProgressCreatedEventHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ModuleProgressCreatedEvent notification, CancellationToken cancellationToken)
        {
            foreach (var articleId in notification.ArticleIds)
            {
                var articleProgress = ArticleProgress.Create(notification.ModuleProgressId, articleId);

                // Nếu là bài đầu tiên được đọc => đánh dấu Progressing luôn
                if (articleId == notification.FirstSectionArticleId)
                {
                    articleProgress.Update(ProcessStatus.Progressing);
                }

                _dbContext.ArticleProgresses.Add(articleProgress);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
