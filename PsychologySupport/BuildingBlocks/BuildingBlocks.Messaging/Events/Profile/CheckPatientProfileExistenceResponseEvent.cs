namespace LifeStyles.API.Events
{
    public record CheckPatientProfileExistenceResponseEvent(Guid PatientProfileId, bool Exists);
}
