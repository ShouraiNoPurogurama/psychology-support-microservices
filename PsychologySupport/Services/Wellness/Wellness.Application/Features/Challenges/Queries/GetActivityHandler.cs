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

namespace Wellness.Application.Features.Challenges.Queries;

public record GetActivitiesQuery(
    ActivityType? ActivityType,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetActivitiesResult>;

public record GetActivitiesResult(PaginatedResult<ActivityDto> Activities);

public class GetActivitiesHandler : IQueryHandler<GetActivitiesQuery, GetActivitiesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetActivitiesHandler(
        IWellnessDbContext context,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetActivitiesResult> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Activities
            .Include(a => a.ChallengeSteps)
            .Include(a => a.ProcessHistories)
            .AsNoTracking()
            .AsQueryable();

        if (request.ActivityType.HasValue)
            query = query.Where(a => a.ActivityType == request.ActivityType.Value);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        if (!activities.Any())
        {
            var empty = new PaginatedResult<ActivityDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                new List<ActivityDto>()
            );
            return new GetActivitiesResult(empty);
        }

        // --- Translation logic using extension ---
        List<Activity> translatedActivities = activities;

        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            try
            {
                translatedActivities = await activities.TranslateEntitiesAsync(
                    nameof(Activity),
                    _translationClient,
                    a => a.Id.ToString(),
                    cancellationToken,
                    a => a.Name,
                    a => a.Description
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationError] {ex.Message}");
            }
        }

        var resultDtos = translatedActivities.Adapt<List<ActivityDto>>();

        var paginatedResult = new PaginatedResult<ActivityDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            resultDtos
        );

        return new GetActivitiesResult(paginatedResult);
    }
}
