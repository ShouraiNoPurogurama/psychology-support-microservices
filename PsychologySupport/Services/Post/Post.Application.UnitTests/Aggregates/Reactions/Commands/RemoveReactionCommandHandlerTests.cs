using FluentAssertions;
using NSubstitute;
using Post.Application.Abstractions.Integration;
using Post.Application.Features.Reactions.Commands.RemoveReaction;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.UnitTests.Aggregates.Reactions.Commands;

public sealed class RemoveReactionCommandHandlerTests
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;
    private readonly IOutboxWriter _outboxWriter;
    private readonly RemoveReactionCommandHandler _handler;

    public RemoveReactionCommandHandlerTests()
    {
        _context = Substitute.For<IPostDbContext>();
        _aliasAccessor = Substitute.For<IAliasVersionAccessor>();
        _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
        _outboxWriter = Substitute.For<IOutboxWriter>();
        _handler = new RemoveReactionCommandHandler(_context, _aliasAccessor, _outboxWriter, _currentActorAccessor);
    }

    [Fact]
    public async Task Handle_ValidPostTarget_ShouldReturnResult()
    {
        // Arrange
        var command = new RemoveReactionCommand(
            ReactionTargetType.Post,
            Guid.NewGuid()
        );

        _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _currentActorAccessor.GetRequiredAliasId().Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RemoveReactionResult>();
    }

    [Theory]
    [InlineData(ReactionTargetType.Post)]
    [InlineData(ReactionTargetType.Comment)]
    public async Task Handle_ValidTargetTypes_ShouldAcceptAllTargetTypes(ReactionTargetType targetType)
    {
        // Arrange
        var command = new RemoveReactionCommand(
            targetType,
            Guid.NewGuid()
        );

        _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _currentActorAccessor.GetRequiredAliasId().Returns(Guid.NewGuid());

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().NotThrowAsync();
    }
}
