
namespace BuildingBlocks.Messaging.Events.Queries.LifeStyle
{
    //public record ActivityRequest(Guid Id, string ActivityType);
    public record ActivityRequest(List<Guid> Ids, string ActivityType);

}
