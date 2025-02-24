using BuildingBlocks.CQRS;
using Feedback.API.Data;

namespace Blog.API.Features.Feedbacks.CreateFeedback;

public record CreateFeedbackCommand(Feedback.API.Models.Feedback Feedback) : ICommand<CreateFeedbackResult>;

public record CreateFeedbackResult(Guid Id);

public class CreateFeedbackHandler : ICommandHandler<CreateFeedbackCommand, CreateFeedbackResult>
{
    private readonly FeedbackDbContext _context;

    public CreateFeedbackHandler(FeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<CreateFeedbackResult> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        _context.Feedbacks.Add(request.Feedback);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateFeedbackResult(request.Feedback.Id);
    }
}
