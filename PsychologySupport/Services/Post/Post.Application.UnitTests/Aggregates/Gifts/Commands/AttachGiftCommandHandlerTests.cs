using FluentAssertions;
using NSubstitute;
using Post.Application.Aggregates.Gifts.Commands.AttachGift;
using Post.Application.Data;
using Post.Application.Abstractions.Authentication;
using Post.Application.Integration;
using Post.Domain.Aggregates.Gifts.Enums;
using Xunit;
using BuildingBlocks.Exceptions;

namespace Post.Application.UnitTests.Aggregates.Gifts.Commands;

public sealed class AttachGiftCommandHandlerTests
{
    private readonly IPostDbContext _context;
    private readonly IAliasVersionResolver _aliasResolver;
    private readonly IActorResolver _actorResolver;
    private readonly IOutboxWriter _outboxWriter;
    private readonly AttachGiftCommandHandler _handler;

    public AttachGiftCommandHandlerTests()
    {
        _context = Substitute.For<IPostDbContext>();
        _aliasResolver = Substitute.For<IAliasVersionResolver>();
        _actorResolver = Substitute.For<IActorResolver>();
        _outboxWriter = Substitute.For<IOutboxWriter>();
        _handler = new AttachGiftCommandHandler(_context, _aliasResolver, _outboxWriter, _actorResolver);
    }

    [Fact]
    public async Task Handle_ValidPostTarget_ShouldAttachGift()
    {
        // Arrange
        var command = new AttachGiftCommand(
            GiftTargetType.Post,
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "Test message"
        );

        _aliasResolver.GetCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _actorResolver.AliasId.Returns(Guid.NewGuid());

        // Act & Assert
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.TargetType.Should().Be(GiftTargetType.Post);
    }

    [Fact]
    public async Task Handle_InvalidTargetType_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new AttachGiftCommand(
            (GiftTargetType)999, // Invalid enum value
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            null
        );

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BadRequestException>();
    }
}
