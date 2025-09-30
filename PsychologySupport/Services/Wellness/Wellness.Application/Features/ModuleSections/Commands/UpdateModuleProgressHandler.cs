using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Domain.Enums;

public record UpdateModuleProgressCommand(
    Guid ModuleSectionId,
    Guid SubjectRef,
    Guid ArticleId
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

            var moduleProgress = await _dbContext.ModuleProgresses
                .Include(mp => mp.ArticleProgresses)
                .FirstOrDefaultAsync(
                    mp => mp.SectionId == request.ModuleSectionId && mp.SubjectRef == request.SubjectRef,
                    cancellationToken);

            if (moduleProgress == null)
                throw new WellnessNotFoundException($"Không tìm thấy ModuleProgress cho SectionId = {request.ModuleSectionId} và SubjectRef = {request.SubjectRef}.");


            var article = await _dbContext.SectionArticles
                .FirstOrDefaultAsync(a => a.Id == request.ArticleId && a.SectionId == request.ModuleSectionId, cancellationToken);

            if (article == null)
                throw new WellnessNotFoundException($"Article {request.ArticleId} không thuộc Section {request.ModuleSectionId}.");

            moduleProgress.Update(request.ArticleId, article.Duration);


            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateModuleProgressResult(
                moduleProgress.Id,
                moduleProgress.ProcessStatus,
                moduleProgress.MinutesRead ?? 0
            );
        }
    }
}
