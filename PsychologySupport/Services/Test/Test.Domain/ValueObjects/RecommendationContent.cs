using Newtonsoft.Json;

namespace Test.Domain.ValueObjects;

public class Recommendation
{
    private Recommendation(string overview, string emotionAnalysis, List<PersonalizedSuggestion> suggestions, string closing)
    {
        Overview = overview;
        EmotionAnalysis = emotionAnalysis;
        PersonalizedSuggestions = suggestions;
        Closing = closing;
    }

    public string Overview { get; }
    public string EmotionAnalysis { get; }
    public List<PersonalizedSuggestion> PersonalizedSuggestions { get; }
    public string Closing { get; }

    public static Recommendation Create(string overview, string emotionAnalysis, List<PersonalizedSuggestion> suggestions, string closing)
    {
        return new Recommendation(overview, emotionAnalysis, suggestions, closing);
    }

    public static Recommendation FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Recommendation>(json)
               ?? throw new InvalidOperationException("Không thể parse Recommendation từ chuỗi JSON.");
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class PersonalizedSuggestion
{
    public PersonalizedSuggestion(string title, string description, List<string> tips, string reference)
    {
        Title = title;
        Description = description;
        Tips = tips;
        Reference = reference;
    }

    public string Title { get; }
    public string Description { get; }
    public List<string> Tips { get; }
    public string Reference { get; }
}