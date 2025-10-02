using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.ProcessHistories.Commands
{
    public record UpdateProcessHistoryCommand(
        Guid SubjectRef,
        Guid ProcessHistoryId,
        ProcessStatus ProcessStatus,
        Guid? PostMoodId
    ) : ICommand<UpdateProcessHistoryResult>;

    public record UpdateProcessHistoryResult(
        Guid ProcessHistoryId,
        ProcessStatus ProcessStatus,
        DateTimeOffset? EndTime,
        Guid? PostMoodId
    );

    internal class UpdateProcessHistoryHandler
        : ICommandHandler<UpdateProcessHistoryCommand, UpdateProcessHistoryResult>
    {
        private readonly IWellnessDbContext _dbContext;

        public UpdateProcessHistoryHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpdateProcessHistoryResult> Handle(
            UpdateProcessHistoryCommand request,
            CancellationToken cancellationToken)
        {
            var processHistory = await _dbContext.ProcessHistories
                .FirstOrDefaultAsync(
                    ph => ph.Id == request.ProcessHistoryId && ph.SubjectRef == request.SubjectRef,
                    cancellationToken);

            if (processHistory == null)
            {
                throw new WellnessNotFoundException(
                    $"Không tìm thấy ProcessHistory với Id = {request.ProcessHistoryId}.");
            }

            // Cập nhật trạng thái
            processHistory.Update(request.ProcessStatus, request.PostMoodId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateProcessHistoryResult(
                processHistory.Id,
                processHistory.ProcessStatus,
                processHistory.EndTime,
                processHistory.PostMoodId
            );
        }
    }
}
