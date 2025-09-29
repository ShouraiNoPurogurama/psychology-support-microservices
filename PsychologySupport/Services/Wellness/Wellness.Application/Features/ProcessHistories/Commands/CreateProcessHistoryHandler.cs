using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Enums;
using Wellness.Domain.Aggregates.ProcessHistories;

namespace Wellness.Application.Features.ProcessHistories.Commands;


public record CreateProcessHistoryCommand(
    Guid IdempotencyKey,
    Guid SubjectRef,
    Guid ActivityId
) : IdempotentCommand<CreateProcessHistoryResult>(IdempotencyKey);


public record CreateProcessHistoryResult(
    Guid ProcessHistoryId,
    ProcessStatus Status,
    DateTimeOffset StartTime
);

internal class CreateProcessHistoryHandler
    : ICommandHandler<CreateProcessHistoryCommand, CreateProcessHistoryResult>
{
    private readonly IWellnessDbContext _dbContext;

    public CreateProcessHistoryHandler(IWellnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProcessHistoryResult> Handle(CreateProcessHistoryCommand request, CancellationToken cancellationToken)
    {

        if (request.ActivityId != Guid.Empty)
        {
            var activity = await _dbContext.Activities
                .FirstOrDefaultAsync(a => a.Id == request.ActivityId, cancellationToken);

            if (activity is null)
                throw new NotFoundException($"Hoạt động không tồn tại.");
        }

        var history = ProcessHistory.Create(request.SubjectRef, request.ActivityId);

        _dbContext.ProcessHistories.Add(history);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProcessHistoryResult(
            history.Id,
            history.ProcessStatus,
            history.StartTime
        );
    }
}
