using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using Wellness.Application.Data;
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

        query = query.Where(cp => cp.SubjectRef == request.SubjectRef);

        if (request.ProcessStatus.HasValue)
            query = query.Where(cp => cp.ProcessStatus == request.ProcessStatus.Value);

        if (request.ChallengeType.HasValue)
            query = query.Where(cp => cp.Challenge!.ChallengeType == request.ChallengeType.Value);

        query = query.OrderByDescending(cp => cp.StartDate)
                     .ThenBy(cp => cp.Challenge!.ChallengeType);

        var totalCount = await query.CountAsync(cancellationToken);

        var rawProgresses = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        // --- Translation ---
        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            var builder = TranslationUtils.CreateBuilder();

            // Challenge fields
            builder.AddEntities(rawProgresses, nameof(ChallengeProgress), x => x.Challenge!.Title);
            builder.AddEntities(rawProgresses, nameof(ChallengeProgress), x => x.Challenge.Description);
            builder.AddEntities(
                rawProgresses.Select(cp => new { cp.Id, ChallengeType = cp.Challenge!.ChallengeType.ToString() }),
                nameof(ChallengeProgress),
                x => x.ChallengeType
            );

            // Activity fields
            var allStepProgresses = rawProgresses.SelectMany(cp => cp.ChallengeStepProgresses).ToList();

            builder.AddEntities(
                allStepProgresses.Select(sp => new { sp.ChallengeStep!.Activity!.Id, sp.ChallengeStep.Activity.Name }),
                "Activity",
                x => x.Name
            );
            builder.AddEntities(
                allStepProgresses.Select(sp => new { sp.ChallengeStep!.Activity!.Id, sp.ChallengeStep.Activity.Description }),
                "Activity",
                x => x.Description
            );
            builder.AddEntities(
                allStepProgresses.Select(sp => new { sp.ChallengeStep!.Activity!.Id, ActivityType = sp.ChallengeStep.Activity.ActivityType.ToString() }),
                "Activity",
                x => x.ActivityType
            );

            var translationDict = builder.Build();

            var translationResponse = await _translationClient.TranslateDataAsync(
                new TranslateDataRequest
                {
                    Originals = { translationDict },
                    TargetLanguage = request.TargetLang
                },
                cancellationToken: cancellationToken
            );

            translations = translationResponse.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // --- Map translated values ---
        var items = rawProgresses.Select(cp =>
        {
            var challenge = cp.Challenge!;

            if (translations != null)
            {
                if (translations.TryGetValue($"ChallengeProgress:{cp.Id}:Title", out var title))
                    challenge.Title = title;
                if (translations.TryGetValue($"ChallengeProgress:{cp.Id}:Description", out var desc))
                    challenge.Description = desc;
                if (translations.TryGetValue($"ChallengeProgress:{cp.Id}:ChallengeType", out var ctStr) &&
                    Enum.TryParse<ChallengeType>(ctStr, out var parsedCT))
                    challenge.ChallengeType = parsedCT;
            }

            var steps = cp.ChallengeStepProgresses.Select(sp =>
            {
                var step = sp.ChallengeStep!;
                var activity = step.Activity;

                if (activity != null && translations != null)
                {
                    if (translations.TryGetValue($"Activity:{activity.Id}:Name", out var name))
                        activity.Name = name;
                    if (translations.TryGetValue($"Activity:{activity.Id}:Description", out var adesc))
                        activity.Description = adesc;
                    if (translations.TryGetValue($"Activity:{activity.Id}:ActivityType", out var atStr) &&
                        Enum.TryParse<ActivityType>(atStr, out var parsedAT))
                        activity.ActivityType = parsedAT;
                }

                return sp.Adapt<ChallengeStepProgressDto>();
            }).OrderBy(sp => sp.DayNumber)
              .ThenBy(sp => sp.OrderIndex)
              .ToList();

            return new ChallengeProgressDto
            {
                Id = cp.Id,
                SubjectRef = cp.SubjectRef,
                ChallengeId = cp.ChallengeId,
                ChallengeTitle = challenge.Title,
                ChallengeDescription = challenge.Description,
                ChallengeType = challenge.ChallengeType.ToString(),
                ProcessStatus = cp.ProcessStatus,
                ProgressPercent = cp.ProgressPercent,
                StartDate = cp.StartDate,
                EndDate = cp.EndDate,
                Steps = steps
            };

        }).ToList();

        var paginated = new PaginatedResult<ChallengeProgressDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            items
        );

        return new GetChallengeProgressResult(paginated);
    }
}
