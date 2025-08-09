
namespace BuildingBlocks.Messaging.Events.LifeStyle
{
    //public record ActivityRequest(Guid Id, string ActivityType);
    public record ActivityRequest(List<Guid> Ids, string ActivityType);

}
