using BuildingBlocks.Exceptions;

namespace Profile.API.Exceptions;

public class ProfileNotFoundException : NotFoundException
{
    public ProfileNotFoundException(string message) : base(message)
    {
    }
}