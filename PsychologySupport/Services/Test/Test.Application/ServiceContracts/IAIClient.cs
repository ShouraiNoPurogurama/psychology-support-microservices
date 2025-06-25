
using Test.Application.Dtos.DASS21Recommendations;
using Test.Domain.ValueObjects;

namespace Test.Application.ServiceContracts;

public interface IAIClient
{
    Task<RecommendationsDto> GetDASS21RecommendationsAsync(
        string patientProfileId,
        Score depressionScore,
        Score anxietyScore,
        Score stressScore
    );
}