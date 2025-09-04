namespace BuildingBlocks.Messaging.Events.Queries.LifeStyle;

public record AggregatePatientLifestyleRequest(Guid ProfileId, DateTime Date);