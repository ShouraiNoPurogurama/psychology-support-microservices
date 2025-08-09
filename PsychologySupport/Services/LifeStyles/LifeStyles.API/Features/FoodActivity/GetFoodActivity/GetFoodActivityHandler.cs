using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Utils;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using LifeStyles.API.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetFoodActivity;

public record GetFoodActivityQuery(Guid Id) : IQuery<GetFoodActivityResult>;

public record GetFoodActivityResult(FoodActivityDto FoodActivity);

public class GetFoodActivityHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient)
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

        var response = await translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi),
            cancellationToken
        );

        var translations = response.Message.Translations;

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
