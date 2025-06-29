using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Utils;

public static class TranslationUtils
{
    //1. Tạo translation dictionary với hỗ trợ multiple entities
    /// <summary>
    /// Converts a collection of objects into a translation dictionary.
    /// Each entry key is in the format: Entity_Property_Id.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="objects">The list of entities to extract data from.</param>
    /// <param name="getId">Function to extract ID from each object.</param>
    /// <param name="entityName">Name of the entity (e.g., Product, Question).</param>
    /// <param name="propertiesToTranslate">List of string properties to extract for translation.</param>
    /// <returns>Dictionary of keys and original string values.</returns>
    public static Dictionary<string, string> ToTranslationDictionary<T>(
        this IEnumerable<T> objects,
        Func<T, string> getId,
        string entityName,
        params Expression<Func<T, string>>[] propertiesToTranslate
    )
    {
        var result = new Dictionary<string, string>();

        foreach (var obj in objects)
        {
            var id = getId(obj);
            if (string.IsNullOrEmpty(id)) continue;

            foreach (var propExpr in propertiesToTranslate)
            {
                var propName = GetPropertyName(propExpr);
                var value = propExpr.Compile().Invoke(obj);

                if (!string.IsNullOrEmpty(value))
                {
                    var key = $"{entityName}_{propName}_{id}";
                    result[key] = value;
                }
            }
        }

        return result;
    }

    //2. Overload với auto-detect Id property
    /// <summary>
    /// Overload: Converts a collection of objects into a translation dictionary.
    /// Auto-detects ID property using reflection (Id, ID, id).
    /// </summary>
    public static Dictionary<string, string> ToTranslationDictionary<T>(
        this IEnumerable<T> objects,
        string entityName,
        params Expression<Func<T, string>>[] propertiesToTranslate
    )
    {
        return objects.ToTranslationDictionary(
            obj => GetIdValue(obj),
            entityName,
            propertiesToTranslate
        );
    }

    //3. Tạo translation dictionary từ nhiều collections khác nhau
    /// <summary>
    /// Merges multiple translation dictionaries into one.
    /// If keys are duplicated, later values overwrite earlier ones.
    /// </summary>
    public static Dictionary<string, string> CombineTranslationDictionaries(
        params Dictionary<string, string>[] dictionaries
    )
    {
        var result = new Dictionary<string, string>();
        
        foreach (var dict in dictionaries)
        {
            foreach (var kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    //4. Map translated properties với improved error handling
    /// <summary>
    /// Maps translated values from the dictionary back into the object.
    /// Requires explicit ID.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="translations">Dictionary of translations.</param>
    /// <param name="obj">Target object to inject translations into.</param>
    /// <param name="entityName">Name of the entity (used to generate keys).</param>
    /// <param name="id">ID value of the object.</param>
    /// <param name="properties">Properties to set translated values for.</param>
    /// <returns>The object with updated values.</returns>
    public static T MapTranslatedProperties<T>(
        this Dictionary<string, string> translations,
        T obj,
        string entityName,
        string id,
        params Expression<Func<T, string>>[] properties
    ) where T : class
    {
        if (obj == null) return obj;

        foreach (var propExpr in properties)
        {
            try
            {
                var propName = GetPropertyName(propExpr);
                var key = $"{entityName}_{propName}_{id}";

                if (translations.TryGetValue(key, out var translatedValue))
                {
                    var propInfo = typeof(T).GetProperty(propName);
                    if (propInfo != null && propInfo.CanWrite)
                    {
                        propInfo.SetValue(obj, translatedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue processing other properties
                Console.WriteLine($"Error mapping property for {entityName}: {ex.Message}");
            }
        }

        return obj;
    }

    //5. Overload với auto-detect Id
    /// <summary>
    /// Overload: Maps translated values from dictionary to object.
    /// Auto-detects ID using reflection.
    /// </summary>
    public static T MapTranslatedProperties<T>(
        this Dictionary<string, string> translations,
        T obj,
        string entityName,
        params Expression<Func<T, string>>[] properties
    ) where T : class
    {
        var id = GetIdValue(obj);
        return translations.MapTranslatedProperties(obj, entityName, id, properties);
    }

    //6. Batch mapping cho collections
    /// <summary>
    /// Batch version of MapTranslatedProperties.
    /// Maps translations for a collection of objects.
    /// </summary>
    public static IEnumerable<T> MapTranslatedPropertiesForCollection<T>(
        this Dictionary<string, string> translations,
        IEnumerable<T> objects,
        string entityName,
        params Expression<Func<T, string>>[] properties
    ) where T : class
    {
        return objects.Select(obj => 
            translations.MapTranslatedProperties(obj, entityName, properties)
        ).ToList();
    }
    
    public static IEnumerable<string> MapTranslatedStrings(
        this Dictionary<string, string> translations,
        IEnumerable<string> originalValues,
        string entityName)
    {
        foreach (var value in originalValues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield return value;
                continue;
            }

            var id = EncodeStringId(value);
            var key = $"{entityName}_Name_{id}";

            yield return translations.GetValueOrDefault(key, value);
        }
    }



    //7. Get translated value with fallback
    /// <summary>
    /// Retrieves translated value for a specific property of an object.
    /// Falls back to original if translation not found.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="translations">Translation dictionary.</param>
    /// <param name="obj">The object being translated.</param>
    /// <param name="propertySelector">Property to look up.</param>
    /// <param name="entityName">Entity name for key generation.</param>
    /// <param name="id">Optional override ID; otherwise auto-detected.</param>
    /// <returns>Translated value if available, else original value.</returns>
    public static string GetTranslatedValue<T>(
        this Dictionary<string, string> translations,
        T obj,
        Expression<Func<T, string>> propertySelector,
        string entityName,
        string? id = null
    )
    {
        try
        {
            var propertyName = GetPropertyName(propertySelector);
            var actualId = id ?? GetIdValue(obj);
            
            if (string.IsNullOrEmpty(actualId))
                return propertySelector.Compile().Invoke(obj);

            var key = $"{entityName}_{propertyName}_{actualId}";
            return translations.TryGetValue(key, out var translatedValue)
                ? translatedValue
                : propertySelector.Compile().Invoke(obj);
        }
        catch
        {
            return propertySelector.Compile().Invoke(obj);
        }
    }

    //8. Helper method để tạo translation request cho multiple entity types
    /// <summary>
    /// Builds translation dictionary from multiple entity groups.
    /// Useful when translating multiple types (e.g., Product, Category).
    /// </summary>
    public static Dictionary<string, string> BuildTranslationRequest(
        params (IEnumerable<object> objects, string entityName, Expression<Func<object, string>>[] properties)[] entityGroups
    )
    {
        var combinedDict = new Dictionary<string, string>();

        foreach (var (objects, entityName, properties) in entityGroups)
        {
            var dict = objects.ToTranslationDictionary(
                obj => GetIdValue(obj),
                entityName,
                properties
            );

            foreach (var kvp in dict)
            {
                combinedDict[kvp.Key] = kvp.Value;
            }
        }

        return combinedDict;
    }

    //9. Fluent API builder cho complex scenarios
    /// <summary>
    /// Fluent builder class to create translation dictionaries.
    /// Supports chaining multiple AddEntities calls.
    /// </summary>
    public class TranslationBuilder
    {
        private readonly Dictionary<string, string> _translationDict = new();

        public TranslationBuilder AddEntities<T>(
            IEnumerable<T> entities,
            string entityName,
            params Expression<Func<T, string>>[] properties)
        {
            var dict = entities.ToTranslationDictionary(entityName, properties);
            foreach (var kvp in dict)
            {
                _translationDict[kvp.Key] = kvp.Value;
            }
            return this;
        }

        public Dictionary<string, string> Build() => _translationDict;
        
        /// //////////////////

        public TranslationBuilder AddStrings(IEnumerable<string> values, string entityName)
        {
            foreach (var value in values.Distinct())
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var id = EncodeStringId(value);
                    var key = $"{entityName}_Name_{id}";
                    _translationDict[key] = value;
                }
            }

            return this;
        }

    }

    /// <summary>
    /// Creates a new instance of the TranslationBuilder.
    /// </summary>
    public static TranslationBuilder CreateBuilder() => new();

    //10.Extension methods cho common scenarios
    /// <summary>
    /// Extension method to simplify full translation lifecycle:
    /// - Build translation dictionary
    /// - Call translation function
    /// - Map results back to object
    /// </summary>
    /// <typeparam name="T">Type of entity.</typeparam>
    /// <param name="objects">Objects to translate.</param>
    /// <param name="entityName">Entity name used in key generation.</param>
    /// <param name="targetLanguage">Language to translate to.</param>
    /// <param name="translateFunc">Async function to translate dictionary.</param>
    /// <param name="properties">Properties to translate.</param>
    /// <returns>Translated object collection.</returns>
    public static async Task<IEnumerable<T>> TranslateAsync<T>(
        this IEnumerable<T> objects,
        string entityName,
        string targetLanguage,
        Func<Dictionary<string, string>, string, Task<Dictionary<string, string>>> translateFunc,
        params Expression<Func<T, string>>[] properties
    ) where T : class
    {
        var translationDict = objects.ToTranslationDictionary(entityName, properties);
        var translations = await translateFunc(translationDict, targetLanguage);
        return translations.MapTranslatedPropertiesForCollection(objects, entityName, properties);
    }

    //Helper methods
    private static string GetIdValue<T>(T obj)
    {
        if (obj == null) return string.Empty;

        // Try common Id property names
        var idProperties = new[] { "Id", "ID", "id" };
        
        foreach (var idPropName in idProperties)
        {
            var idProp = typeof(T).GetProperty(idPropName);
            if (idProp != null)
            {
                var idValue = idProp.GetValue(obj);
                if (idValue != null)
                {
                    return idValue.ToString();
                }
            }
        }

        var encodedId = EncodeStringId(obj);

        return encodedId;
    }

    private static string EncodeStringId<T>([DisallowNull] T obj)
    {
        //Fallback: generate a stable hash from serialized object (e.g. JSON)
        var serialized = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var bytes = Encoding.UTF8.GetBytes(serialized);
        var base64 = Convert.ToBase64String(bytes);

        //Replace unsafe URL characters
        var result = base64.Replace("=", "").Replace("/", "_").Replace("+", "-");
        return result;
    }

    private static string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression unary when unary.Operand is MemberExpression memberExpr => memberExpr.Member.Name,
            _ => throw new ArgumentException("Invalid expression")
        };
    }
}

//11. Strongly-typed translation context
public class TranslationContext<T> where T : class
{
    private readonly Dictionary<string, string> _translations;
    private readonly string _entityName;

    public TranslationContext(Dictionary<string, string> translations, string entityName)
    {
        _translations = translations;
        _entityName = entityName;
    }

    public T Translate(T obj, params Expression<Func<T, string>>[] properties)
    {
        return _translations.MapTranslatedProperties(obj, _entityName, properties);
    }

    public IEnumerable<T> TranslateCollection(IEnumerable<T> objects, params Expression<Func<T, string>>[] properties)
    {
        return _translations.MapTranslatedPropertiesForCollection(objects, _entityName, properties);
    }
    
    
}

//12. Factory for creating translation contexts
public static class TranslationContextFactory
{
    public static TranslationContext<T> Create<T>(
        Dictionary<string, string> translations,
        string entityName = null
    ) where T : class
    {
        entityName ??= typeof(T).Name.Replace("Dto", "");
        return new TranslationContext<T>(translations, entityName);
    }
}

