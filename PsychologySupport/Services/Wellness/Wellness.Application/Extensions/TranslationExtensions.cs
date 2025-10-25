using BuildingBlocks.Enums;
using BuildingBlocks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Translation.API.Protos;

namespace Wellness.Application.Extensions
{
    public static class TranslationExtensions
    {
        public static async Task<List<T>> TranslateEntitiesAsync<T>(
            this List<T> entities,
            string entityName,
            TranslationService.TranslationServiceClient translationClient,
            Func<T, string> idSelector,
            CancellationToken cancellationToken,
            params Expression<Func<T, string>>[] properties
        ) where T : class
        {
            var translationDict = entities.ToTranslationDictionary(
                idSelector,
                entityName,
                properties
            );

            var translationRequest = new TranslateDataRequest
            {
                Originals = { translationDict },
                TargetLanguage = SupportedLang.vi.ToString()
            };

            var translationResponse = await translationClient.TranslateDataAsync(
                translationRequest,
                cancellationToken: cancellationToken
            );

            var translations = translationResponse.Translations.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );

            var translatedEntities = entities
                .Select(e => translations.MapTranslatedProperties(
                    e,
                    entityName,
                    id: idSelector(e),
                    properties
                ))
                .ToList();

            return translatedEntities;
        }

        public static async Task<T> TranslateEntityAsync<T>(
            this T entity,
            string entityName,
            TranslationService.TranslationServiceClient translationClient,
            Func<T, string> idSelector,
            CancellationToken cancellationToken,
            params Expression<Func<T, string>>[] properties
        ) where T : class
        {
            var dict = new List<T> { entity }.ToTranslationDictionary(
                idSelector,
                entityName,
                properties
            );

            var translationRequest = new TranslateDataRequest
            {
                Originals = { dict },
                TargetLanguage = SupportedLang.vi.ToString()
            };

            var translationResponse = await translationClient.TranslateDataAsync(
                translationRequest,
                cancellationToken: cancellationToken
            );

            var translations = translationResponse.Translations.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );

            return translations.MapTranslatedProperties(entity, entityName, idSelector(entity), properties);
        }
    }
}
