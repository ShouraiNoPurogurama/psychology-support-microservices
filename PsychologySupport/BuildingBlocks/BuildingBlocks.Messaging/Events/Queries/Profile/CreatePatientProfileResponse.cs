namespace BuildingBlocks.Messaging.Events.Queries.Profile
{
    public record CreatePatientProfileResponse(Guid PatientId, bool Success, string? Message = null);

}
