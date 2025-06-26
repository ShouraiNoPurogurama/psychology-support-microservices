namespace Test.Application.Dtos.DASS21Recommendations;

public record CreateRecommendationResponseDto(
    RecommendationsDto Recommendation,
    string PatientName,
    int PatientAge);