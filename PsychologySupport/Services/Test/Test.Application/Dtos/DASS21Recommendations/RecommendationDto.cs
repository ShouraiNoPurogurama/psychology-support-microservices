namespace Test.Application.Dtos.DASS21Recommendations;

public record RecommendationsDto(
    string Overview,
    string EmotionAnalysis,
    List<PersonalizedSuggestionDto> PersonalizedSuggestions,
    string Closing
);

public record PersonalizedSuggestionDto(
    string Title,
    string Description,
    List<string> Tips,
    string Reference
);
