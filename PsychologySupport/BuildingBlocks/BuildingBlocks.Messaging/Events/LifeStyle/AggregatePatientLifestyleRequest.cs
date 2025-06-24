namespace BuildingBlocks.Messaging.Events.LifeStyle;

public record AggregatePatientLifestyleRequest(Guid ProfileId, DateTime Date);