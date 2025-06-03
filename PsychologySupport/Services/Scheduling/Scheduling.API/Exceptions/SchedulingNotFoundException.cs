using BuildingBlocks.Exceptions;

namespace Scheduling.API.Exceptions
{
    public class ScheduleNotFoundException : NotFoundException
    {
        public ScheduleNotFoundException(string? message) : base(message)
        {
        }

        public ScheduleNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
