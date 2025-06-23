using BuildingBlocks.Messaging.Events.LifeStyle;
using BuildingBlocks.Messaging.Events.Profile;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Test.Domain.ValueObjects;

namespace Test.Application.ServiceContracts;

public interface IAIClient
{
    Task<string> GetDASS21RecommendationsAsync(
        string patientProfileId,
        Score depressionScore,
        Score anxietyScore,
        Score stressScore
    );
}