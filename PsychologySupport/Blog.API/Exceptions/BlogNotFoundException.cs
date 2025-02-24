using BuildingBlocks.Exceptions;

namespace Blog.API.Exceptions
{
    public class BlogNotFoundException : NotFoundException
    {
        public BlogNotFoundException(string? message) : base(message)
        {
        }
        public BlogNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {

        }
    }
}
