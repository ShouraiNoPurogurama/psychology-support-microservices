using FluentAssertions;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Exceptions;
using Xunit;

namespace Post.Domain.UnitTests.Aggregates.Gifts;

public class GiftAttachTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateGiftAttach()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 5L;
        var message = "Congratulations!";

        // Act
        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Assert
        giftAttach.Should().NotBeNull();
        giftAttach.Id.Should().NotBe(Guid.Empty);
        giftAttach.Target.TargetType.Should().Be(targetType);
        giftAttach.Target.TargetId.Should().Be(targetId);
        giftAttach.Info.GiftId.Should().Be(giftId);
        giftAttach.Sender.AliasId.Should().Be(senderAliasId);
        giftAttach.Sender.AliasVersionId.Should().Be(senderAliasVersionId);
        giftAttach.Amount.Should().Be(amount);
        giftAttach.Message.Should().Be(message);
        giftAttach.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        giftAttach.IsDeleted.Should().BeFalse();
        giftAttach.DeletedAt.Should().BeNull();
        giftAttach.DeletedByAliasId.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullMessage_ShouldCreateGiftAttachWithNullMessage()
    {
        // Arrange
        var targetType = "Comment";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 10L;
        string? message = null;

        // Act
        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Assert
        giftAttach.Should().NotBeNull();
        giftAttach.Message.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyMessage_ShouldCreateGiftAttachWithNullMessage()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 1L;
        var message = "   ";  // Whitespace only

        // Act
        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Assert
        giftAttach.Should().NotBeNull();
        giftAttach.Message.Should().BeNull();
    }

    [Fact]
    public void Create_WithMessageWithExtraWhitespace_ShouldTrimMessage()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 1L;
        var message = "  Thank you!  ";  // Extra whitespace

        // Act
        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Assert
        giftAttach.Should().NotBeNull();
        giftAttach.Message.Should().Be("Thank you!");  // Trimmed
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowInvalidGiftDataException()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = -1L;  // Negative amount
        var message = "Test";

        // Act & Assert
        FluentActions.Invoking(() => GiftAttach.Create(
                targetType,
                targetId,
                giftId,
                senderAliasId,
                senderAliasVersionId,
                amount,
                message
            ))
            .Should().Throw<InvalidGiftDataException>()
            .WithMessage("Giá trị quà tặng phải lớn hơn 0.");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrowInvalidGiftDataException()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 0L;  // Zero amount
        var message = "Test";

        // Act & Assert
        FluentActions.Invoking(() => GiftAttach.Create(
                targetType,
                targetId,
                giftId,
                senderAliasId,
                senderAliasVersionId,
                amount,
                message
            ))
            .Should().Throw<InvalidGiftDataException>()
            .WithMessage("Giá trị quà tặng phải lớn hơn 0.");
    }

    [Fact]
    public void Create_WithNullAliasVersionId_ShouldCreateGiftAttachWithNullAliasVersionId()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        Guid? senderAliasVersionId = null;  // Null alias version ID
        var amount = 5L;
        var message = "Test";

        // Act
        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Assert
        giftAttach.Should().NotBeNull();
        giftAttach.Sender.AliasVersionId.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldMarkAsDeleted()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 5L;
        var message = "Test";
        var deleterAliasId = Guid.NewGuid();

        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // Act
        giftAttach.SoftDelete(deleterAliasId);

        // Assert
        giftAttach.IsDeleted.Should().BeTrue();
        giftAttach.DeletedAt.Should().NotBeNull();
        giftAttach.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        giftAttach.DeletedByAliasId.Should().Be(deleterAliasId.ToString());
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldNotChangeDeletedProperties()
    {
        // Arrange
        var targetType = "Post";
        var targetId = Guid.NewGuid();
        var giftId = Guid.NewGuid();
        var senderAliasId = Guid.NewGuid();
        var senderAliasVersionId = Guid.NewGuid();
        var amount = 5L;
        var message = "Test";
        var firstDeleterAliasId = Guid.NewGuid();
        var secondDeleterAliasId = Guid.NewGuid();

        var giftAttach = GiftAttach.Create(
            targetType,
            targetId,
            giftId,
            senderAliasId,
            senderAliasVersionId,
            amount,
            message
        );

        // First delete
        giftAttach.SoftDelete(firstDeleterAliasId);
        var originalDeletedAt = giftAttach.DeletedAt;
        var originalDeletedByAliasId = giftAttach.DeletedByAliasId;

        // Wait a moment to ensure timestamps would be different
        System.Threading.Thread.Sleep(10);

        // Act - try to delete again
        giftAttach.SoftDelete(secondDeleterAliasId);

        // Assert - properties should not change
        giftAttach.IsDeleted.Should().BeTrue();
        giftAttach.DeletedAt.Should().Be(originalDeletedAt);
        giftAttach.DeletedByAliasId.Should().Be(originalDeletedByAliasId);
        giftAttach.DeletedByAliasId.Should().NotBe(secondDeleterAliasId.ToString());
    }
}
