using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Enums;

public record CreateModuleProgressCommand(
    Guid IdempotencyKey,
    Guid SubjectRef,         // Ai đang học (User/Subject)
    Guid ModuleSectionId,          // Section nào
    Guid SectionArticleId    // Bài viết đầu tiên được đọc
) : IdempotentCommand<CreateModuleProgressResult>(IdempotencyKey);

public record CreateModuleProgressResult(
    Guid ModuleProgressId,
    ProcessStatus ProcessStatus,
    int MinutesRead
);

namespace Wellness.Application.Features.ModuleSections.Commands
{
    internal class CreateModuleProgressHandler
        : ICommandHandler<CreateModuleProgressCommand, CreateModuleProgressResult>
    {
        private readonly IWellnessDbContext _dbContext;

        public CreateModuleProgressHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateModuleProgressResult> Handle(CreateModuleProgressCommand request, CancellationToken cancellationToken)
        {
          
            var existing = await _dbContext.ModuleProgresses
                .FirstOrDefaultAsync(mp => mp.SubjectRef == request.SubjectRef
                                           && mp.SectionId == request.ModuleSectionId,
                                     cancellationToken);

            if (existing is not null)
            {
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MODULE_PROGRESS_EXISTS"] = new[] { "ModuleProgress cho Section này đã tồn tại." }
                });
            }

      
            var section = await _dbContext.ModuleSections
                .Include(s => s.SectionArticles)
                .FirstOrDefaultAsync(s => s.Id == request.ModuleSectionId, cancellationToken);

            if (section == null)
                throw new NotFoundException($"Không tìm thấy ModuleSection với Id = {request.ModuleSectionId}.");

     
            var articleIds = section.SectionArticles.Select(a => a.Id).ToList();
            if (!articleIds.Any())
            {
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["ARTICLE_IDS_REQUIRED"] = new[] { "Section này không có bài viết nào." }
                });
            }

         
            var firstArticle = section.SectionArticles.FirstOrDefault(a => a.Id == request.SectionArticleId);
            if (firstArticle == null)
            {
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["SECTION_ARTICLE_INVALID"] = new[] { "SectionArticleId không thuộc về Section này." }
                });
            }

 
            var firstArticleDuration = firstArticle.Duration;

            // 6. Tạo mới ModuleProgress qua domain method
            var moduleProgress = ModuleProgress.Create(
                request.SubjectRef,
                request.ModuleSectionId,
                articleIds,
                request.SectionArticleId,
                firstArticleDuration
            );

            _dbContext.ModuleProgresses.Add(moduleProgress);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateModuleProgressResult(
                moduleProgress.Id,
                moduleProgress.ProcessStatus,
                moduleProgress.MinutesRead ?? 0
            );
        }
    }
}
