using FluentValidation;

namespace Post.Application.Features.Comments.Queries.GetCommentById;

public sealed class GetCommentByIdQueryValidator : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("CommentId is required.");
    }
}
