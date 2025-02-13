using BuildingBlocks.CQRS;
using Feedback.API.Data;
using Feedback.API.Exceptions;

namespace Blog.API.Features.Feedbacks.DeleteFeedback;

public record DeleteFeedbackCommand(Guid Id) : ICommand<DeleteFeedbackResult>;

public record DeleteFeedbackResult(bool IsSuccess);

public class DeleteFeedbackHandler : ICommandHandler<DeleteFeedbackCommand, DeleteFeedbackResult>
{
    private readonly FeedbackDbContext _context;

    public DeleteFeedbackHandler(FeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteFeedbackResult> Handle(DeleteFeedbackCommand request, CancellationToken cancellationToken)
    {
        var existingFeedback = await _context.Feedbacks.FindAsync(request.Id)
                              ?? throw new FeedbackNotFoundException("Feedback", request.Id);

        _context.Feedbacks.Remove(existingFeedback);

        var result = await _context.SaveChangesAsync() > 0;

        return new DeleteFeedbackResult(result);
    }
}
