using BuildingBlocks.DDD;


namespace Test.Domain.Events
{
    public record TestResultCreatedEvent(Guid TestResultId, List<Guid> SelectedOptionIds) : IDomainEvent;
}
