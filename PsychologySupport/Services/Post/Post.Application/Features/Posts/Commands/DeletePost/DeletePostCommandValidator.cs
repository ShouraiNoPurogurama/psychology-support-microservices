using FluentValidation;

namespace Post.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Vui lòng chọn bài viết cần xóa.");
    }
}
