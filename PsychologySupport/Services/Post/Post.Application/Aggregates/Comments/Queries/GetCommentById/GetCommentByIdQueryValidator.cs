using FluentValidation;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentById;

public sealed class GetCommentByIdQueryValidator : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("CommentId is required.");
    }
}
