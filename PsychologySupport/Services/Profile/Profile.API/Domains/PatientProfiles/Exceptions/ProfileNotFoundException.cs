namespace Profile.API.Domains.PatientProfiles.Exceptions;

public class ProfileNotFoundException : NotFoundException
{
    public ProfileNotFoundException(string? message) : base(message)
    {
    }

    public ProfileNotFoundException(string name, Guid id) : base($"Không tìm thấy hồ sơ \"{name}\" với Id là {id}.")
    {
    }
    
    public ProfileNotFoundException(Guid id) : base($"Không tìm thấy hồ sơ với chữ ID là {id}.")
    {
    }
}