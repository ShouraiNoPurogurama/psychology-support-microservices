using System.Linq.Expressions;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Utils;

namespace Profile.API.Extensions;

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
}