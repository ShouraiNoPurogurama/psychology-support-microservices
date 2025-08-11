using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Utils;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using LifeStyles.API.Models;
using Microsoft.EntityFrameworkCore;
using Translation.API.Protos;

namespace LifeStyles.API.Features.FoodActivity.GetFoodActivity;

public record GetFoodActivityQuery(Guid Id) : IQuery<GetFoodActivityResult>;

public record GetFoodActivityResult(FoodActivityDto FoodActivity);

public class GetFoodActivityHandler(
    LifeStylesDbContext context,
    TranslationService.TranslationServiceClient translationClient)
    : IQueryHandler<GetFoodActivityQuery, GetFoodActivityResult>
{
    public async Task<GetFoodActivityResult> Handle(GetFoodActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await context.FoodActivities
            .Include(fa => fa.FoodNutrients)
            .Include(fa => fa.FoodCategories)
            .FirstOrDefaultAsync(fa => fa.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Food Activity", request.Id);

        // Map sang DTO
        var activityDto = new FoodActivityDto(
            activity.Id,
            activity.Name,
            activity.Description,
            activity.MealTime.ToReadableString(),
            activity.FoodNutrients.Select(fn => fn.Name),
            activity.FoodCategories.Select(fc => fc.Name),
            activity.IntensityLevel.ToReadableString()
        );

        // Build translation dict
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities([activityDto], nameof(Models.FoodActivity), x => x.Name, x => x.Description, x => x.IntensityLevel)
            .AddStrings(activityDto.FoodNutrients, nameof(Models.FoodNutrient))
            .AddStrings(activityDto.FoodCategories, nameof(FoodCategory))
            .Build();

        // Chuyển đổi translationDict thành TranslateDataRequest
        var translateRequest = new TranslateDataRequest
        {
            Originals = { translationDict },
            TargetLanguage = SupportedLang.vi.ToString()
        };

        // Gọi gRPC TranslateData
        var response = await translationClient.TranslateDataAsync(translateRequest, cancellationToken: cancellationToken);

        var translations = response.Translations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var translatedDto = activityDto with
        {
            Name = translations.GetTranslatedValue(activityDto, x => x.Name, nameof(Models.FoodActivity)),
            Description = translations.GetTranslatedValue(activityDto, x => x.Description, nameof(Models.FoodActivity)),
            IntensityLevel = translations.GetTranslatedValue(activityDto, x => x.IntensityLevel, nameof(Models.FoodActivity)),
            FoodNutrients = translations.MapTranslatedStrings(activityDto.FoodNutrients, nameof(Models.FoodNutrient)),
            FoodCategories = translations.MapTranslatedStrings(activityDto.FoodCategories, nameof(FoodCategory))
        };

        return new GetFoodActivityResult(translatedDto);
    }
}