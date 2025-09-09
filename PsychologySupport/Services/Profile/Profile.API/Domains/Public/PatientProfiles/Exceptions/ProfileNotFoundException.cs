namespace Profile.API.Domains.PatientProfiles.Exceptions;

public class ProfileNotFoundException : NotFoundException
{
    private const string Code = "PROFILE_NOT_FOUND";
    private const string DefaultSafeMessage = "Không tìm thấy hồ sơ của người dùng.";

    public ProfileNotFoundException()
        : base(
            errorCode: Code,
            safeMessage: DefaultSafeMessage,
            internalDetail: null
        )
    {
    }

    public ProfileNotFoundException(string safeMessage, string? errorCode = null, string? internalDetail = null)
        : base(
            errorCode: errorCode ?? Code,
            safeMessage: safeMessage,
            internalDetail: internalDetail
        )
    {
    }
}