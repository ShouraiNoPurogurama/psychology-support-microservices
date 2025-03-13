namespace BuildingBlocks.Messaging.Events.Profile;

public record GetDoctorProfileRequest(Guid DoctorId, Guid? UserId = null);