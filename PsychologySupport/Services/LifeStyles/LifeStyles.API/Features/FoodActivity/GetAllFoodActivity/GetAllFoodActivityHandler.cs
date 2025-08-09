using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using LifeStyles.API.Models;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesQuery(
    int PageIndex,
    int PageSize,
    string? Search
) : IQuery<GetAllFoodActivitiesResult>;

public record GetAllFoodActivitiesResult(PaginatedResult<FoodActivityDto> FoodActivities);

public class GetAllFoodActivityHandler : IQueryHandler<GetAllFoodActivitiesQuery, GetAllFoodActivitiesResult>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    // private readonly IRedisCache _redisCache;

    public GetAllFoodActivityHandler(LifeStylesDbContext context, IRequestClient<GetTranslatedDataRequest> translationClient
        // , IRedisCache redisCache
    )
    {
        _context = context;
        _translationClient = translationClient;
        // _redisCache = redisCache;
    }

    public async Task<GetAllFoodActivitiesResult> Handle(GetAllFoodActivitiesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.FoodActivities
            .Include(fa => fa.FoodNutrients)
            .Include(fa => fa.FoodCategories)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(fa =>
                fa.Name.Contains(request.Search) ||
                fa.FoodNutrients.Any(n => n.Name.Contains(request.Search)) ||
                fa.FoodCategories.Any(c => c.Name.Contains(request.Search))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rawActivities = await query
            .OrderBy(fa => fa.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<FoodActivityDto>()
            .ToListAsync(cancellationToken);

        //Tạo dict dịch cho name, nutrient, category
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawActivities, nameof(Models.FoodActivity), x => x.Name)
            .AddStrings(rawActivities.SelectMany(x => x.FoodNutrients), nameof(Models.FoodNutrient))
            .AddStrings(rawActivities.SelectMany(x => x.FoodCategories), nameof(FoodCategory))
            .Build();

        var response = await _translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi), cancellationToken);

        var translations = response.Message.Translations;

        //Map lại vào DTO
        var translatedActivities = rawActivities.Select(a =>
        {
            var name = translations.GetTranslatedValue(a, x => x.Name, nameof(Models.FoodActivity));

            var nutrients = translations.MapTranslatedStrings(a.FoodNutrients, nameof(Models.FoodNutrient)).ToList();
            var categories = translations.MapTranslatedStrings(a.FoodCategories, nameof(FoodCategory)).ToList();

            return a with
            {
                Name = name,
                FoodNutrients = nutrients,
                FoodCategories = categories
            };
        }).ToList();

        var result = new PaginatedResult<FoodActivityDto>(
            pageIndex, pageSize, totalCount, translatedActivities);

        return new GetAllFoodActivitiesResult(result);
    }
    
    private record SimpleTextDto(string Id)
    {
        public string Name => Id;
    }
}