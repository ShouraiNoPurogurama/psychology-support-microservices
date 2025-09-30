using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Domain.Enums;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.Application.Features.Challenges.Queries;

public record GetChallengeProgressQuery(
    Guid SubjectRef,
    ProcessStatus? ProcessStatus,
    ChallengeType? ChallengeType,
    int PageIndex = 1,
    int PageSize = 10
) : IQuery<GetChallengeProgressResult>;

public record GetChallengeProgressResult(PaginatedResult<ChallengeProgressDto> ChallengeProgresses);

internal class GetChallengeProgressHandler
    : IQueryHandler<GetChallengeProgressQuery, GetChallengeProgressResult>
{
    private readonly IWellnessDbContext _context;

    public GetChallengeProgressHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetChallengeProgressResult> Handle(GetChallengeProgressQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChallengeProgresses
            .Include(cp => cp.Challenge)!.ThenInclude(c => c.ChallengeSteps)!.ThenInclude(s => s.Activity)
            .Include(cp => cp.ChallengeStepProgresses)!.ThenInclude(sp => sp.ChallengeStep)!.ThenInclude(s => s.Activity)
            .AsNoTracking()
            .AsQueryable();

        // mandatory filter by SubjectRef
        query = query.Where(cp => cp.SubjectRef == request.SubjectRef);

        if (request.ProcessStatus.HasValue)
        {
            query = query.Where(cp => cp.ProcessStatus == request.ProcessStatus.Value);
        }

        if (request.ChallengeType.HasValue)
        {
            query = query.Where(cp => cp.Challenge!.ChallengeType == request.ChallengeType.Value);
        }

        // sort by StartDate desc then ChallengeType asc
        query = query
            .OrderByDescending(cp => cp.StartDate)
            .ThenBy(cp => cp.Challenge!.ChallengeType);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(cp => new ChallengeProgressDto
            {
                Id = cp.Id,
                SubjectRef = cp.SubjectRef,
                ChallengeId = cp.ChallengeId,
                ChallengeTitle = cp.Challenge!.Title,
                ChallengeDescription = cp.Challenge.Description,
                ChallengeType = cp.Challenge.ChallengeType.ToString(),
                ProcessStatus = cp.ProcessStatus,
                ProgressPercent = cp.ProgressPercent,
                StartDate = cp.StartDate,
                EndDate = cp.EndDate,
                Steps = cp.ChallengeStepProgresses
                    .OrderBy(sp => sp.ChallengeStep!.DayNumber)       // sort DayNumber
                    .ThenBy(sp => sp.ChallengeStep!.OrderIndex)       // sort OrderIndex
                    .Select(sp => new ChallengeStepProgressDto
                 {
                    StepId = sp.ChallengeStepId,
                    DayNumber = sp.ChallengeStep!.DayNumber,
                    OrderIndex = sp.ChallengeStep.OrderIndex,
                    Activity = sp.ChallengeStep.Activity == null
                        ? null
                        : new ActivityDto(
                            sp.ChallengeStep.Activity.Id,
                            sp.ChallengeStep.Activity.Name,
                            sp.ChallengeStep.Activity.Description,
                            sp.ChallengeStep.Activity.ActivityType,
                            sp.ChallengeStep.Activity.Duration,
                            sp.ChallengeStep.Activity.Instructions
                        ),

                    ProcessStatus = sp.ProcessStatus,
                    StartedAt = sp.StartedAt,
                    CompletedAt = sp.CompletedAt,
                    PostMoodId = sp.PostMoodId
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        var paginated = new PaginatedResult<ChallengeProgressDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            items
        );

        return new GetChallengeProgressResult(paginated);
    }
}
