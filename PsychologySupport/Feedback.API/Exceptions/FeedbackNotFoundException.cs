using BuildingBlocks.Exceptions;

namespace Feedback.API.Exceptions
{
    public class FeedbackNotFoundException : NotFoundException
    {
        public FeedbackNotFoundException(string? message) : base(message)
        {
        }
        public FeedbackNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {

        }
    }
}
