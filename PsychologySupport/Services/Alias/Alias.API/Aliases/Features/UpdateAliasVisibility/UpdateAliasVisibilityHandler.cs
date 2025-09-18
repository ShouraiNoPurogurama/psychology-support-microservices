using Alias.API.Aliases.Exceptions;
using Alias.API.Aliases.Models.Enums;
using Alias.API.Common.Authentication;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using FluentValidation;
using MassTransit;

namespace Alias.API.Aliases.Features.UpdateAliasVisibility;

public record UpdateAliasVisibilityCommand(AliasVisibility Visibility)
    : ICommand<UpdateAliasVisibilityResult>;

public record UpdateAliasVisibilityResult(
    Guid AliasId,
    AliasVisibility Visibility,
    DateTimeOffset UpdatedAt);

public sealed class UpdateAliasVisibilityCommandValidator : AbstractValidator<UpdateAliasVisibilityCommand>
{
    public UpdateAliasVisibilityCommandValidator()
    {
        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Giá trị visibility không hợp lệ.")
            .NotEqual(AliasVisibility.Suspended)
            .WithMessage("Không thể tự đặt trạng thái suspended.");
    }
}

public class UpdateAliasVisibilityHandler(
    AliasDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ICurrentActorAccessor currentActorAccessor)
    : ICommandHandler<UpdateAliasVisibilityCommand, UpdateAliasVisibilityResult>
{
    public async Task<UpdateAliasVisibilityResult> Handle(UpdateAliasVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        currentActorAccessor.TryGetAliasId(out var aliasId);

        if (aliasId == Guid.Empty)
            throw new AliasNotFoundException("Không tìm thấy hồ sơ người dùng.", "ALIAS_NOT_FOUND");

        // Load alias aggregate
        var alias = await dbContext.Aliases
                        .Include(a => a.Versions)
                        .Include(a => a.AuditRecords)
                        .FirstOrDefaultAsync(a => a.Id == aliasId && !a.IsDeleted, cancellationToken)
                    ?? throw new AliasNotFoundException("Không tìm thấy hồ sơ người dùng để cập nhật đối tượng hiển thị.",
                        "ALIAS_NOT_FOUND");

        // Use domain aggregate method to change visibility
        alias.ChangeVisibility(command.Visibility);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish integration event for cross-service communication
        var aliasVisibilityChangedEvent = new AliasVisibilityChangedIntegrationEvent(
            alias.Id,
            alias.Visibility.ToString(),
            DateTimeOffset.UtcNow);

        await publishEndpoint.Publish(aliasVisibilityChangedEvent, cancellationToken);

        return new UpdateAliasVisibilityResult(
            alias.Id,
            alias.Visibility,
            DateTimeOffset.UtcNow);
    }
}