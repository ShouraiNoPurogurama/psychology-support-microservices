using FluentAssertions;
using NSubstitute;
using Post.Application.Abstractions.Integration;
using Post.Application.Features.Reactions.Commands.CreateReaction;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.UnitTests.Aggregates.Reactions.Commands;

public sealed class CreateReactionCommandHandlerTests
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;
    private readonly CreateReactionCommandHandler _handler;

    public CreateReactionCommandHandlerTests()
    {
        _context = Substitute.For<IPostDbContext>();
        _aliasAccessor = Substitute.For<IAliasVersionAccessor>();
        _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
        _outboxWriter = Substitute.For<IOutboxWriter>();
        _handler = new CreateReactionCommandHandler(_context, _aliasAccessor, _outboxWriter, _currentActorAccessor);
    }

    [Fact]
    public async Task Handle_ValidPostTarget_ShouldCreateReaction()
    {
        // Arrange
        var command = new CreateReactionCommand(
            ReactionTargetType.Post,
            Guid.NewGuid(),
            ReactionCode.Like
        );

        _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _currentActorAccessor.GetRequiredAliasId().Returns(Guid.NewGuid());

        // Act & Assert
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.TargetType.Should().Be(ReactionTargetType.Post);
        result.ReactionCode.Should().Be(ReactionCode.Like);
    }

    [Theory]
    [InlineData(ReactionCode.Like)]
    [InlineData(ReactionCode.Heart)]
    [InlineData(ReactionCode.Laugh)]
    public async Task Handle_ValidReactionCodes_ShouldCreateReactionWithCorrectCode(ReactionCode reactionCode)
    {
        // Arrange
        var command = new CreateReactionCommand(
            ReactionTargetType.Post,
            Guid.NewGuid(),
            reactionCode
        );

        _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _currentActorAccessor.GetRequiredAliasId().Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ReactionCode.Should().Be(reactionCode);
    }
}
