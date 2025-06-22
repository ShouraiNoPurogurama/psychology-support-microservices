using BuildingBlocks.Dtos; 

namespace BuildingBlocks.Messaging.Events.LifeStyle
{

    public record ActivityRequestResponse<T>(List<T> Activities) where T : IActivityDto;

}
