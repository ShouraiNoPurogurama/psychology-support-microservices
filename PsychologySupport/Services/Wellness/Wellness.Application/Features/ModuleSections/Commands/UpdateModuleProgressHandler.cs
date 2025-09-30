using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Domain.Enums;

public record UpdateModuleProgressCommand(
    Guid ModuleSectionId,
    Guid SubjectRef,
    Guid SectionArticleId
) : ICommand<UpdateModuleProgressResult>;

public record UpdateModuleProgressResult(
    Guid ModuleProgressId,
    ProcessStatus ProcessStatus,
    int MinutesRead
);


namespace Wellness.Application.Features.ModuleSections.Commands
{
    internal class UpdateModuleProgressHandler
        : ICommandHandler<UpdateModuleProgressCommand, UpdateModuleProgressResult>
    {
        private readonly IWellnessDbContext _dbContext;

        public UpdateModuleProgressHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpdateModuleProgressResult> Handle(UpdateModuleProgressCommand request, CancellationToken cancellationToken)
        {
            // Lấy module progress
            var moduleProgress = await _dbContext.ModuleProgresses
                .Include(mp => mp.ArticleProgresses)
                .Include(mp => mp.Section) // để lấy TotalDuration
                .FirstOrDefaultAsync(
                    mp => mp.SectionId == request.ModuleSectionId && mp.SubjectRef == request.SubjectRef,
                    cancellationToken);

            if (moduleProgress == null)
                throw new WellnessNotFoundException(
                    $"Không tìm thấy ModuleProgress cho SectionId = {request.ModuleSectionId} và SubjectRef = {request.SubjectRef}.");

            if (moduleProgress.ProcessStatus == ProcessStatus.Completed)
            {
                return new UpdateModuleProgressResult(
                    moduleProgress.Id,
                    moduleProgress.ProcessStatus,
                    moduleProgress.MinutesRead ?? 0
                );
            }

            // Lấy bài viết
            var article = await _dbContext.SectionArticles
                .FirstOrDefaultAsync(a => a.Id == request.SectionArticleId && a.SectionId == request.ModuleSectionId, cancellationToken);

            if (article == null)
                throw new WellnessNotFoundException(
                    $"Article {request.SectionArticleId} không thuộc Section {request.ModuleSectionId}.");

            // Kiểm tra nếu Article đã hoàn thành rồi thì không cộng thêm thời gian
            var articleProgress = moduleProgress.ArticleProgresses
                .FirstOrDefault(ap => ap.ArticleId == request.SectionArticleId);

            if (articleProgress != null && articleProgress.ProcessStatus == ProcessStatus.Completed)
            {
                return new UpdateModuleProgressResult(
                    moduleProgress.Id,
                    moduleProgress.ProcessStatus,
                    moduleProgress.MinutesRead ?? 0
                );
            }

            // Cập nhật module progress
            moduleProgress.Update(request.SectionArticleId, article.Duration);

            // Nếu tổng MinutesRead >= TotalDuration thì đánh dấu Completed
            if (moduleProgress.MinutesRead >= moduleProgress.Section!.TotalDuration)
            {
                moduleProgress.ProcessStatus = ProcessStatus.Completed;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateModuleProgressResult(
                moduleProgress.Id,
                moduleProgress.ProcessStatus,
                moduleProgress.MinutesRead ?? 0);
        }
    }
}
