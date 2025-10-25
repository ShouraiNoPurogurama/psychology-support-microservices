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

        var challenges = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        if (!challenges.Any())
        {
            var empty = new PaginatedResult<ChallengeDto>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                new List<ChallengeDto>()
            );
            return new GetChallengesResult(empty);
        }

        // --- Apply Translation using Extension ---
        List<Challenge> translatedChallenges = challenges;

        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            try
            {
                // Dịch tiêu đề + mô tả của Challenge
                translatedChallenges = await challenges.TranslateEntitiesAsync(
                    nameof(Challenge),
                    _translationClient,
                    c => c.Id.ToString(),
                    cancellationToken,
                    c => c.Title,
                    c => c.Description
                );

                // Dịch tên và mô tả của Activity trong từng Step
                foreach (var challenge in translatedChallenges)
                {
                    var activities = challenge.ChallengeSteps
                        .Where(s => s.Activity != null)
                        .Select(s => s.Activity!)
                        .ToList();

                    if (activities.Any())
                    {
                        var translatedActivities = await activities.TranslateEntitiesAsync(
                            nameof(Activity),
                            _translationClient,
                            a => a.Id.ToString(),
                            cancellationToken,
                            a => a.Name,
                            a => a.Description
                        );

                        // Gán lại bản dịch vào step
                        foreach (var step in challenge.ChallengeSteps)
                        {
                            var translated = translatedActivities.FirstOrDefault(a => a.Id == step.Activity?.Id);
                            if (translated != null)
                                step.Activity = translated;
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
        var challengeDtos = translatedChallenges.Adapt<List<ChallengeDto>>();

        var paginated = new PaginatedResult<ChallengeDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            challengeDtos
        );

        return new GetChallengesResult(paginated);
    }
}
