using BuildingBlocks.Exceptions;

namespace Image.API.Exceptions
{
    public class ImageNotFoundException : NotFoundException
    {
        public ImageNotFoundException(string? message) : base(message)
        {
        }

        public ImageNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
