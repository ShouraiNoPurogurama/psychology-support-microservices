namespace Test.Application.Dtos.DASS21Recommendations;

public record CreateRecommendationResponseDto(
    RecommendationsDto Recommendation,
    string PatientName,
    string ProfileNickname,
    string ProfileDescription,
    List<string> ProfileHighlights,
    int PatientAge);