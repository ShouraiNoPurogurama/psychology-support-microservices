using System.Linq.Expressions;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Utils;
using MassTransit;

namespace Subscription.API.Extensions;

public static class TranslationExtensions
{
    public static async Task<List<T>> TranslateEntitiesAsync<T>(
        this List<T> entities,
        string entityName,
        IRequestClient<GetTranslatedDataRequest> translationClient,
        Func<T, string> idSelector,
        CancellationToken cancellationToken,
        params Expression<Func<T, string>>[] properties) where T : class
    {
        var translationDict = entities.ToTranslationDictionary(
            idSelector,
            entityName,
            properties
        );
        var translationResponse = await translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi), cancellationToken);

        var translations = translationResponse.Message.Translations;

        var translatedEntities = entities
            .Select(e => translations.MapTranslatedProperties(e, entityName, id: idSelector(e), properties))
            .ToList();

        return translatedEntities;
    }
    
    public static async Task<T> TranslateEntityAsync<T>(
        this T entity,
        string entityName,
        IRequestClient<GetTranslatedDataRequest> translationClient,
        CancellationToken cancellationToken,
        params Expression<Func<T, string>>[] properties
    ) where T : class
    {
        var dict = new List<T> { entity }.ToTranslationDictionary(entityName, properties);

        var response = await translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(dict, SupportedLang.vi), cancellationToken);

        var translations = response.Message.Translations;

        return translations.MapTranslatedProperties(entity, entityName, properties);
    }

}