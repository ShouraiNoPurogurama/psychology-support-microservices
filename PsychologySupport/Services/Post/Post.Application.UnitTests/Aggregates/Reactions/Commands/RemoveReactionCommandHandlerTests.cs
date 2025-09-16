using FluentAssertions;
using NSubstitute;
using Post.Application.Aggregates.Reactions.Commands.RemoveReaction;
using Post.Application.Data;
using Post.Application.Abstractions.Authentication;
using Post.Application.Integration;
using Post.Domain.Aggregates.Reactions.Enums;
using Xunit;

namespace Post.Application.UnitTests.Aggregates.Reactions.Commands;

public sealed class RemoveReactionCommandHandlerTests
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;
    private readonly RemoveReactionCommandHandler _handler;

    public RemoveReactionCommandHandlerTests()
    {
        _context = Substitute.For<IPostDbContext>();
        _aliasResolver = Substitute.For<IAliasVersionResolver>();
        _actorResolver = Substitute.For<IActorResolver>();
        _outboxWriter = Substitute.For<IOutboxWriter>();
        _handler = new RemoveReactionCommandHandler(_context, _aliasResolver, _outboxWriter, _actorResolver);
    }

    [Fact]
    public async Task Handle_ValidPostTarget_ShouldReturnResult()
    {
        // Arrange
        var command = new RemoveReactionCommand(
            ReactionTargetType.Post,
            Guid.NewGuid()
        );

        _aliasResolver.GetCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _actorResolver.AliasId.Returns(Guid.NewGuid());

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

        _aliasResolver.GetCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _actorResolver.AliasId.Returns(Guid.NewGuid());

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().NotThrowAsync();
    }
}
