using FluentValidation;

namespace Post.Application.Aggregates.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MaximumLength(2000)
            .WithMessage("Comment cannot exceed 2,000 characters");

        RuleFor(x => x.ParentCommentId)
            .NotEqual(Guid.Empty)
            .WithMessage("Parent comment ID cannot be empty GUID")
            .When(x => x.ParentCommentId.HasValue);
    }
}
