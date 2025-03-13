namespace BuildingBlocks.Messaging.Events.Profile;

public record GetPatientProfileRequest(Guid PatientId, Guid? UserId = null);