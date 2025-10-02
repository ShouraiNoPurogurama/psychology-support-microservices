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

public record GetChallengesQuery(
    ChallengeType? ChallengeType,
    PaginationRequest PaginationRequest,
    string? TargetLang = null
) : IQuery<GetChallengesResult>;

public record GetChallengesResult(PaginatedResult<ChallengeDto> Challenges);

public class GetChallengesHandler : IQueryHandler<GetChallengesQuery, GetChallengesResult>
{
    private readonly IWellnessDbContext _context;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetChallengesHandler(
        IWellnessDbContext context,
        TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetChallengesResult> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Challenges
            .Include(c => c.ChallengeSteps)
                .ThenInclude(s => s.Activity)
            .AsNoTracking()
            .AsQueryable();

        if (request.ChallengeType.HasValue)
            query = query.Where(c => c.ChallengeType == request.ChallengeType.Value);

        query = query.OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var rawChallenges = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        Dictionary<string, string>? translations = null;
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            // Tạo string tạm cho ChallengeType và ActivityType
            var challengesWithString = rawChallenges.Select(c => new
            {
                Challenge = c,
                ChallengeTypeString = c.ChallengeType.ToString(),
                Steps = c.ChallengeSteps.Select(s => new
                {
                    Step = s,
                    ActivityTypeString = s.Activity?.ActivityType.ToString()
                }).ToList()
            }).ToList();

            var translationDict = TranslationUtils.CreateBuilder()
                .AddEntities(challengesWithString, nameof(Challenge), x => x.Challenge.Title)
                .AddEntities(challengesWithString, nameof(Challenge), x => x.Challenge.Description)
                .AddEntities(challengesWithString, nameof(Challenge), x => x.ChallengeTypeString)
                // Translate Activities
                .AddEntities(challengesWithString.SelectMany(c => c.Steps), nameof(Activity), x => x.Step.Activity!.Name)
                .AddEntities(challengesWithString.SelectMany(c => c.Steps), nameof(Activity), x => x.Step.Activity!.Description)
                .AddEntities(challengesWithString.SelectMany(c => c.Steps), nameof(Activity), x => x.ActivityTypeString)
                .Build();

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

        var translatedChallenges = rawChallenges.Select(c =>
        {
            // Translate Challenge
            var translatedChallenge = translations?.MapTranslatedProperties(
                c,
                nameof(Challenge),
                id: c.Id.ToString(),
                x => x.Title,
                x => x.Description,
                _ => c.ChallengeType.ToString()
            ) ?? c;

            // Convert ChallengeType back
            if (translations != null &&
                translations.TryGetValue($"{nameof(Challenge)}:{c.Id}:ChallengeTypeString", out var ctStr) &&
                Enum.TryParse<ChallengeType>(ctStr, out var parsedChallengeType))
            {
                translatedChallenge.ChallengeType = parsedChallengeType;
            }

            // Translate Activities inside Steps
            var steps = translatedChallenge.ChallengeSteps.Select(s =>
            {
                if (s.Activity != null && translations != null)
                {
                    var keyName = $"{nameof(Activity)}:{s.Activity.Id}:Name";
                    var keyDesc = $"{nameof(Activity)}:{s.Activity.Id}:Description";
                    var keyType = $"{nameof(Activity)}:{s.Activity.Id}:ActivityTypeString";

                    if (translations.TryGetValue(keyName, out var name)) s.Activity.Name = name;
                    if (translations.TryGetValue(keyDesc, out var desc)) s.Activity.Description = desc;
                    if (translations.TryGetValue(keyType, out var atStr) &&
                        Enum.TryParse<ActivityType>(atStr, out var parsedType))
                        s.Activity.ActivityType = parsedType;
                }

                return s.Adapt<ChallengeStepDto>();
            }).ToList();

            return translatedChallenge.Adapt<ChallengeDto>() with { Steps = steps };
        }).ToList();

        var paginated = new PaginatedResult<ChallengeDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            translatedChallenges
        );

        return new GetChallengesResult(paginated);
    }
}
