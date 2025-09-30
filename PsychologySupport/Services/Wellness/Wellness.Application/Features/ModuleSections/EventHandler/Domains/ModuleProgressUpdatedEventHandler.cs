using MediatR;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;
using static Wellness.Domain.Events.ModuleSectionDomainEvents;


namespace Wellness.Application.Features.ModuleSections.EventHandler.Domains
{
    internal class ModuleProgressUpdatedEventHandler
        : INotificationHandler<ModuleProgressUpdatedEvent>
    {
        private readonly IWellnessDbContext _dbContext;

        public ModuleProgressUpdatedEventHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ModuleProgressUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var articleProgress = await _dbContext.ArticleProgresses
                .FirstOrDefaultAsync(ap =>
                    ap.ModuleProgressId == notification.ModuleProgressId &&
                    ap.ArticleId == notification.ArticleId,
                    cancellationToken);

            if (articleProgress == null)
            {
                // Nếu chưa có thì tạo mới
                articleProgress = ArticleProgress.Create(notification.ModuleProgressId, notification.ArticleId);
                _dbContext.ArticleProgresses.Add(articleProgress);
            }

            articleProgress.Update(ProcessStatus.Completed);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
