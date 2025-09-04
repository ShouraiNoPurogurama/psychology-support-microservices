using BuildingBlocks.Dtos;

namespace BuildingBlocks.Messaging.Events.Queries.LifeStyle
{

    public record ActivityRequestResponse<T>(List<T> Activities) where T : IActivityDto;

}
