
using Test.Application.Dtos.DASS21Recommendations;
using Test.Domain.ValueObjects;

namespace Test.Application.ServiceContracts;

public interface IAIClient
{
    Task<CreateRecommendationResponseDto> GetDASS21RecommendationsAsync(
        Guid subjectRef,
        Score depressionScore,
        Score anxietyScore,
        Score stressScore
    );
}