namespace Alias.API.Common.Authentication;

public interface ICurrentActorAccessor
{
    bool TryGetAliasId(out Guid aliasId);
    Guid GetRequiredAliasId();
    
    bool TryGetSubjectRef(out Guid subjectRef);
    Guid GetRequiredSubjectRef();
}