namespace Profile.API.Exceptions;

public class ProfileNotFoundException : NotFoundException
{
    public ProfileNotFoundException(string? message) : base(message)
    {
    }

    public ProfileNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
    {
    }
}