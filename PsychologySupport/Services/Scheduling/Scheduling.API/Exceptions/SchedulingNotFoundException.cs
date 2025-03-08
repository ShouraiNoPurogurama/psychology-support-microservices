using BuildingBlocks.Exceptions;

namespace Scheduling.API.Exceptions
{
    public class SchedulingNotFoundException : NotFoundException
    {
        public SchedulingNotFoundException(string? message) : base(message)
        {
        }

        public SchedulingNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
