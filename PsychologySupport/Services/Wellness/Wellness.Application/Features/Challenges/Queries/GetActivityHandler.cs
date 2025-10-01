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

namespace Wellness.Application.Features.Challenges.Queries;

public record GetActivitiesQuery(
    ActivityType? ActivityType,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetActivitiesResult>;

public record GetActivitiesResult(PaginatedResult<ActivityDto> Activities);

public class GetActivityHandler : IQueryHandler<GetActivitiesQuery, GetActivitiesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetActivityHandler(
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

        var rawActivities = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        // --- Build translation dictionary ---
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawActivities, nameof(Activity), a => a.Name)
            .AddEntities(rawActivities, nameof(Activity), a => a.Description)
            .AddEntities(
                rawActivities.Select(a => new { a.Id, ActivityTypeString = a.ActivityType.ToString() }),
                nameof(Activity),
                x => x.ActivityTypeString
            )
            .Build();

        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            try
            {
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
            catch (Exception ex)
            {
                // fallback: không dịch, chỉ log lỗi
                Console.WriteLine($"[TranslationError] {ex.Message}");
            }
        }

        // --- Apply translations ---
        var translatedActivities = rawActivities.Select(a =>
        {
            var translated = translations?.MapTranslatedProperties(
                a,
                nameof(Activity),
                id: a.Id.ToString(),
                x => x.Name,
                x => x.Description
            ) ?? a;

            // Map ActivityTypeString
            if (translations != null &&
                translations.TryGetValue($"{nameof(Activity)}:{a.Id}:ActivityTypeString", out var atStr) &&
                Enum.TryParse<ActivityType>(atStr, out var parsed))
            {
                translated.ActivityType = parsed;
            }

            return translated.Adapt<ActivityDto>();
        }).ToList();

        var paginatedResult = new PaginatedResult<ActivityDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            translatedActivities
        );

        return new GetActivitiesResult(paginatedResult);
    }
}
