namespace Post.Application.Abstractions.Authentication;

public interface ICurrentActorAccessor
{
    bool TryGetAliasId(out Guid aliasId);
    Guid GetRequiredAliasId();
}