using BuildingBlocks.CQRS;
using FluentValidation;
using Feedback.API.Exceptions;
using Feedback.API.Data;

namespace Blog.API.Features.Feedbacks.GetFeedback;

public record GetFeedbackQuery(Guid Id) : IQuery<GetFeedbackResult>;

public record GetFeedbackResult(Feedback.API.Models.Feedback Feedback);

public class GetFeedbackQueryValidator : AbstractValidator<GetFeedbackQuery>
{
    public GetFeedbackQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("ID cannot be null or empty");
    }
}

public class GetFeedbackHandler : IQueryHandler<GetFeedbackQuery, GetFeedbackResult>
{
    private readonly FeedbackDbContext _context;

    public GetFeedbackHandler(FeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<GetFeedbackResult> Handle(GetFeedbackQuery query, CancellationToken cancellationToken)
    {
        var feedback = await _context.Feedbacks.FindAsync(query.Id)
                       ?? throw new FeedbackNotFoundException(query.Id.ToString());

        return new GetFeedbackResult(feedback);
    }
}
