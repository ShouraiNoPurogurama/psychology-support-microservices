using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;
using Wellness.Application.Data;
using Wellness.Application.Extensions;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Domain.Aggregates.JournalMoods;

namespace Wellness.Application.Features.JournalMoods.Queries;

public record GetMoodsQuery(string? TargetLang = null) : IQuery<GetMoodsResult>;

public record GetMoodsResult(IEnumerable<MoodDto> Moods);

// Handler
internal class GetMoodsHandler : IQueryHandler<GetMoodsQuery, GetMoodsResult>
{
    private readonly IWellnessDbContext _context;
    private readonly TranslationService.TranslationServiceClient _translationClient;

    public GetMoodsHandler(IWellnessDbContext context, TranslationService.TranslationServiceClient translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetMoodsResult> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ Lấy danh sách moods
        var moods = await _context.Moods
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        // 2️⃣ Nếu có yêu cầu dịch, gọi TranslationService
        if (!string.IsNullOrEmpty(request.TargetLang))
        {
            try
            {
                moods = await moods.TranslateEntitiesAsync(
                    nameof(Mood),                      
                    _translationClient,               
                    m => m.Id.ToString(),             
                    cancellationToken,
                    m => m.Name,                       
                    m => m.Description
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslationError:Mood] {ex.Message}");
            }
        }

        // 3️⃣ Map sang DTO
        var moodDtos = moods.Select(m => new MoodDto(
            m.Id,
            m.Name,
            m.IconCode,
            m.Description
        ));

        return new GetMoodsResult(moodDtos);
    }
}
