using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using Wellness.Application.Data;
using Wellness.Application.Extensions;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.Challenges.Queries;

public record GetChallengeProgressQuery(
    Guid SubjectRef,
    ProcessStatus? ProcessStatus,
    ChallengeType? ChallengeType,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetChallengeProgressResult>;

public record GetChallengeProgressResult(PaginatedResult<ChallengeProgressDto> ChallengeProgresses);

public class GetChallengeProgressHandler : IQueryHandler<GetChallengeProgressQuery, GetChallengeProgressResult>
{
    private readonly IWellnessDbContext _context;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetChallengeProgressHandler(
        IWellnessDbContext context,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetChallengeProgressResult> Handle(GetChallengeProgressQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChallengeProgresses
            .Include(cp => cp.Challenge)!.ThenInclude(c => c.ChallengeSteps)!.ThenInclude(s => s.Activity)
            .Include(cp => cp.ChallengeStepProgresses)!.ThenInclude(sp => sp.ChallengeStep)!.ThenInclude(s => s.Activity)
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        query = query.Where(cp => cp.SubjectRef == request.SubjectRef);

        if (request.ProcessStatus.HasValue)
            query = query.Where(cp => cp.ProcessStatus == request.ProcessStatus.Value);

        if (request.ChallengeType.HasValue)
            query = query.Where(cp => cp.Challenge!.ChallengeType == request.ChallengeType.Value);

        // Sorting
        query = query.OrderByDescending(cp => cp.StartDate)
                     .ThenBy(cp => cp.Challenge!.ChallengeType);

        var totalCount = await query.CountAsync(cancellationToken);

        var rawProgresses = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        if (!rawProgresses.Any())
        {
            var empty = new PaginatedResult<ChallengeProgressDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                new List<ChallengeProgressDto>()
            );
            return new GetChallengeProgressResult(empty);
        }

        // Khởi tạo danh sách uniqueActivities trước
        List<Activity> uniqueActivities = new();

        if (!string.IsNullOrEmpty(request.TargetLang) && request.TargetLang == "vi")
        {
            try
            {
                // Unique challenges
                var uniqueChallenges = rawProgresses
                    .Select(cp => cp.Challenge!)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                // Dịch Challenge Title & Description
                await uniqueChallenges.TranslateEntitiesAsync(
                    nameof(Challenge),
                    _translationClient,
                    c => c.Id.ToString(),
                    cancellationToken,
                    c => c.Title,
                    c => c.Description
                );

                // Unique activities
                uniqueActivities = rawProgresses
                    .SelectMany(cp => cp.Challenge!.ChallengeSteps
                        .Where(cs => cs.Activity != null)
                        .Select(cs => cs.Activity!))
                    .Concat(rawProgresses.SelectMany(cp => cp.ChallengeStepProgresses
                        .Where(csp => csp.ChallengeStep?.Activity != null)
                        .Select(csp => csp.ChallengeStep!.Activity!)))
                    .GroupBy(a => a.Id)
                    .Select(g => g.First())
                    .ToList();

                // Dịch Activity Name & Description
                if (uniqueActivities.Any())
                {
                    await uniqueActivities.TranslateEntitiesAsync(
                        "Activity",
                        _translationClient,
                        a => a.Id.ToString(),
                        cancellationToken,
                        a => a.Name,
                        a => a.Description
                    );

                    // Gán bản dịch vào entity gốc
                    var activityDict = uniqueActivities.ToDictionary(a => a.Id, a => a);
                    foreach (var cp in rawProgresses)
                    {
                        foreach (var step in cp.Challenge!.ChallengeSteps)
                        {
                            if (step.Activity != null && activityDict.TryGetValue(step.Activity.Id, out var translated))
                                step.Activity = translated;
                        }

                        foreach (var stepProgress in cp.ChallengeStepProgresses)
                        {
                            if (stepProgress.ChallengeStep?.Activity != null &&
                                activityDict.TryGetValue(stepProgress.ChallengeStep.Activity.Id, out var translated))
                                stepProgress.ChallengeStep.Activity = translated;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationError] {ex.Message}");
            }
        }

        // --- Map to DTO ---
        var items = rawProgresses.Select(cp =>
        {
            var challenge = cp.Challenge!;
            var challengeTypeStr = challenge.ChallengeType.ToString();

            var steps = cp.ChallengeStepProgresses
                .Select(sp =>
                {
                    var dto = sp.Adapt<ChallengeStepProgressDto>();
                    if (sp.ChallengeStep?.Activity != null)
                    {
                        dto = dto with { Activity = sp.ChallengeStep.Activity.Adapt<ActivityDto>() };
                    }
                    return dto;
                })
                .OrderBy(sp => sp.DayNumber)
                .ThenBy(sp => sp.OrderIndex)
                .ToList();

            return new ChallengeProgressDto
            {
                Id = cp.Id,
                SubjectRef = cp.SubjectRef,
                ChallengeId = cp.ChallengeId,
                ChallengeTitle = challenge.Title,
                ChallengeDescription = challenge.Description,
                ChallengeType = challengeTypeStr,
                ProcessStatus = cp.ProcessStatus,
                ProgressPercent = cp.ProgressPercent,
                StartDate = cp.StartDate,
                EndDate = cp.EndDate,
                Steps = steps
            };
        }).ToList();

        // --- Pagination result ---
        var paginated = new PaginatedResult<ChallengeProgressDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            items
        );

        return new GetChallengeProgressResult(paginated);
    }
}
