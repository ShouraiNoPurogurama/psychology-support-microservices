namespace Test.Application.ServiceContracts;

public interface ICurrentActorAccessor
{
    bool TryGetAliasId(out Guid aliasId);
    Guid GetRequiredAliasId();
    
    bool TryGetSubjectRef(out Guid subjectRef);
    Guid GetRequiredSubjectRef();
    
    bool TryGetPatientId(out Guid patientId);
    Guid GetRequiredPatientId();
}