using FluentAssertions;
using FluentValidation.TestHelper;
using Post.Application.Aggregates.Posts.Commands.CreatePost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.CreatePost;

public class CreatePostCommandValidatorTests
{
    private readonly CreatePostCommandValidator _validator = new();

    [Fact]
    public void ValidateCreatePostCommandWithValidData()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Post Title",
            Content: "This is valid post content with sufficient length",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { Guid.NewGuid() }
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidateCreatePostCommandWithEmptyContent()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Title",
            Content: "",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ValidateCreatePostCommandWithNullContent()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Title",
            Content: null!,
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ValidateCreatePostCommandWithExcessivelyLongContent()
    {
        var longContent = new string('a', 10001); // Assuming max length is 10000
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Title",
            Content: longContent,
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ValidateCreatePostCommandWithEmptyIdempotencyKey()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.Empty,
            Title: "Valid Title",
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }

    [Fact]
    public void ValidateCreatePostCommandWithDuplicateMediaIds()
    {
        var mediaId = Guid.NewGuid();
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Title",
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { mediaId, mediaId }
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MediaIds);
    }

    [Fact]
    public void ValidateCreatePostCommandWithEmptyMediaIds()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Valid Title",
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { Guid.Empty }
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MediaIds);
    }

    [Fact]
    public void ValidateCreatePostCommandWithNullTitle()
    {
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: null,
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void ValidateCreatePostCommandWithExcessivelyLongTitle()
    {
        var longTitle = new string('a', 256); // Assuming max title length is 255
        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: longTitle,
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
