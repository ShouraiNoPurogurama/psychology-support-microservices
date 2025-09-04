namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetDoctorProfileRequest(Guid DoctorId, Guid? UserId = null);