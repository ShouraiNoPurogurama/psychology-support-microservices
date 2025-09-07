using BuildingBlocks.Exceptions;

namespace Media.API.Exceptions
{
    public class MediaNotFoundException : NotFoundException
    {
        public MediaNotFoundException(string? message) : base(message)
        {
        }

        public MediaNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
