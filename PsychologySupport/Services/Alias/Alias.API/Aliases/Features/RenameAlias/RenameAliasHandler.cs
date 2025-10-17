using Alias.API.Aliases.Exceptions;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Aliases.Utils;
using Alias.API.Common.Authentication;
using Alias.API.Data.Public;
using AIModeration.API.Protos;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using MassTransit;
using Pii.API.Protos;

namespace Alias.API.Aliases.Features.RenameAlias;

public record RenameAliasCommand(Guid SubjectRef, string NewLabel) : ICommand<RenameAliasResult>;

public record RenameAliasResult(
    Guid AliasId,
    Guid AliasVersionId,
    string Label,
    string Visibility,
    DateTimeOffset UpdatedAt);

public class RenameAliasHandler(
    AliasDbContext dbContext,
    ModerationService.ModerationServiceClient moderationClient,
    IPublishEndpoint publishEndpoint,
    ILogger<RenameAliasHandler> logger,
    ICurrentActorAccessor currentActorAccessor)
    : ICommandHandler<RenameAliasCommand, RenameAliasResult>
{
    public async Task<RenameAliasResult> Handle(RenameAliasCommand command, CancellationToken cancellationToken)
    {
        currentActorAccessor.TryGetAliasId(out var aliasId);

        if (aliasId == Guid.Empty)
            throw new AliasNotFoundException("Không tìm thấy hồ sơ người dùng để đổi tên.");

        // Validate alias label with AIModeration service
        var moderationRequest = new ValidateAliasLabelRequest { Label = command.NewLabel };
        var moderationResult = await moderationClient.ValidateAliasLabelAsync(moderationRequest, cancellationToken: cancellationToken);
        
        if (!moderationResult.IsValid)
        {
            var reasons = string.Join(", ", moderationResult.Reasons);
            throw new AliasConflictException($"Label không hợp lệ: {reasons}");
        }

        // Check for label uniqueness
        var normalizedUniqueKey = AliasNormalizerUtils.ToUniqueKey(command.NewLabel);
        var labelTaken = await dbContext.AliasVersions
            .AsNoTracking()
            .AnyAsync(v => v.UniqueKey == normalizedUniqueKey, cancellationToken);

        if (labelTaken)
            throw new AliasConflictException("Label đã được sử dụng.");

        // Load alias aggregate
        var alias = await dbContext.Aliases.Include(alias => alias.Label)
                        .Include(alias => alias.CurrentVersion!)
                        .Include(a => a.Versions)
                        .Include(a => a.AuditRecords)
                        .FirstOrDefaultAsync(a => a.Id == aliasId && !a.IsDeleted, cancellationToken)
                    ?? throw new AliasNotFoundException("Không tìm thấy alias để đổi tên.");

        // Use domain aggregate method to update label
        alias.UpdateLabel(command.NewLabel, NicknameSource.Custom);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            // Publish integration event for cross-service communication
            var aliasUpdatedEvent = new AliasUpdatedIntegrationEvent(
                alias.Id,
                command.SubjectRef,
                alias.CurrentVersionId!.Value,
                alias.Label.Value,
                DateTimeOffset.UtcNow);

            await publishEndpoint.Publish(aliasUpdatedEvent, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new AliasConflictException("Label đã được sử dụng.", internalDetail: ex.Message);
        }

        var currentVersion = alias.CurrentVersion!;
        return new RenameAliasResult(
            alias.Id,
            currentVersion.Id,
            currentVersion.DisplayName,
            alias.Visibility.ToString(),
            DateTimeOffset.UtcNow);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
}