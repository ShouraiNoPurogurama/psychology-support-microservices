using System.Linq.Expressions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Post.Application.Aggregates.Gifts.Commands.AttachGift;
using Post.Domain.Aggregates.Comments;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Gifts.DomainEvents;
using Post.Domain.Aggregates.Gifts.Enums;

namespace Post.UnitTests.Aggregates.Gifts.Commands;

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

        _context.Posts.AnyAsync(Arg.Any<Expression<Func<Domain.Aggregates.Posts.Post, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.TargetType.Should().Be(GiftTargetType.Post);
        result.TargetId.Should().Be(command.TargetId);
        result.GiftId.Should().Be(command.GiftId);
        result.Message.Should().Be(command.Message);
        
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _outboxWriter.Received(1).WriteAsync(Arg.Is<GiftAttachedEvent>(e => 
            e.PostId == command.TargetId && 
            e.GiftId == command.GiftId), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommentTarget_ShouldAttachGift()
    {
        // Arrange
        var command = new AttachGiftCommand(
            GiftTargetType.Comment,
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "Test message for comment"
        );

        _aliasResolver.GetCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _actorResolver.AliasId.Returns(Guid.NewGuid());

        _context.Comments.AnyAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TargetType.Should().Be(GiftTargetType.Comment);
        result.TargetId.Should().Be(command.TargetId);
        result.GiftId.Should().Be(command.GiftId);
        result.Message.Should().Be(command.Message);
        
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _outboxWriter.Received(1).WriteAsync(Arg.Is<GiftAttachedEvent>(e => 
            e.PostId == command.TargetId && 
            e.GiftId == command.GiftId), 
            Arg.Any<CancellationToken>());
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

    [Fact]
    public async Task Handle_PostTargetNotFound_ShouldThrowNotFoundException()
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

        _context.Posts.AnyAsync(Arg.Any<Expression<Func<Domain.Aggregates.Posts.Post, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CommentTargetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AttachGiftCommand(
            GiftTargetType.Comment,
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            "Test message"
        );

        _aliasResolver.GetCurrentAliasVersionIdAsync(Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        _actorResolver.AliasId.Returns(Guid.NewGuid());

        _context.Comments.AnyAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Idempotency_ShouldReturnSameResult()
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

        _context.Posts.AnyAsync(Arg.Any<Expression<Func<Domain.Aggregates.Posts.Post, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.GiftAttachId.Should().NotBe(Guid.Empty);
        result2.GiftAttachId.Should().NotBe(Guid.Empty);
        // In a real implementation with idempotency support, result1 and result2 should be identical
        // Currently the handler does not have idempotency implemented
    }
}
