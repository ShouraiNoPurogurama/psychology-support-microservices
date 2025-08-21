using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using LifeStyles.API.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;

namespace LifeStyles.API.Features02.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesV2Query(
    int PageIndex,
    int PageSize,
    string? Search
) : IQuery<GetAllFoodActivitiesV2Result>;

public record GetAllFoodActivitiesV2Result(PaginatedResult<FoodActivityDto> FoodActivities);

public class GetAllFoodActivitiesV2Handler(
    LifeStylesDbContext context,
    TranslationService.TranslationServiceClient translationClient) 
    : IQueryHandler<GetAllFoodActivitiesV2Query, GetAllFoodActivitiesV2Result>
{
    private readonly LifeStylesDbContext _context = context;
    private readonly TranslationService.TranslationServiceClient _translationClient = translationClient;

    public async Task<GetAllFoodActivitiesV2Result> Handle(
        GetAllFoodActivitiesV2Query request,
        CancellationToken cancellationToken)
    {
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
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<FoodActivityDto>()
            .ToListAsync(cancellationToken);

        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawActivities, nameof(Models.FoodActivity), x => x.Name)
            .AddStrings(rawActivities.SelectMany(x => x.FoodNutrients), nameof(Models.FoodNutrient))
            .AddStrings(rawActivities.SelectMany(x => x.FoodCategories), nameof(FoodCategory))
            .Build();

        // Chuyển đổi translationDict thành TranslateDataRequest
        var translateRequest = new TranslateDataRequest
        {
            Originals = { translationDict },
            TargetLanguage = SupportedLang.vi.ToString()
        };

        // Gọi gRPC TranslateData
        var response = await _translationClient.TranslateDataAsync(translateRequest, cancellationToken: cancellationToken);

        var translations = response.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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
            request.PageIndex, request.PageSize, totalCount, translatedActivities);

        return new GetAllFoodActivitiesV2Result(result);
    }
}