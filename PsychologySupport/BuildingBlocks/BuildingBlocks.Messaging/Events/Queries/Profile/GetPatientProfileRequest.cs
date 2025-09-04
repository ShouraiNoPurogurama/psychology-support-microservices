namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetPatientProfileRequest(Guid PatientId, Guid? UserId = null);