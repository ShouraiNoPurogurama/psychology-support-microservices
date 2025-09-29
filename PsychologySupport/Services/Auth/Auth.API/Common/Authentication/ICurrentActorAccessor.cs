namespace Auth.API.Common.Authentication;


    public interface ICurrentActorAccessor
    {
        bool TryGetSubjectRef(out Guid userId);
        Guid GetRequiredSubjectRef();
    }
