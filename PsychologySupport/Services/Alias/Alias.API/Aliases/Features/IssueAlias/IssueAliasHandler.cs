using Alias.API.Aliases.Exceptions;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Aliases.Utils;
using Alias.API.Common.Authentication;
using Alias.API.Common.Security;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Utils;
using MassTransit;

namespace Alias.API.Aliases.Features.IssueAlias;

public record IssueAliasCommand(Guid SubjectRef, string? ReservationToken, string Label)
    : ICommand<IssueAliasResult>;

public record IssueAliasResult(
    Guid AliasId,
    Guid AliasVersionId,
    string Label,
    string Visibility,
    DateTimeOffset CreatedAt);

public class IssueAliasHandler(
    AliasDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IAliasTokenService aliasTokenService,
    ICurrentActorAccessor currentActorAccessor)
    : ICommandHandler<IssueAliasCommand, IssueAliasResult>
{
    public async Task<IssueAliasResult> Handle(IssueAliasCommand command, CancellationToken cancellationToken)
    {
        var isAliasExist = currentActorAccessor.TryGetAliasId(out var id);

        if (isAliasExist)
            throw new AliasConflictException("Người dùng đã có bí danh, không thể tạo mới.");

        var nicknameSource = ExtractNicknameSourceFromToken(command.ReservationToken, command.Label);

        var normalizedUniqueKey = AliasNormalizerUtils.ToUniqueKey(command.Label);
        
        var labelTaken = await dbContext.AliasVersions
            .AsNoTracking()
            .AnyAsync(v => v.UniqueKey == normalizedUniqueKey, cancellationToken);

        if (labelTaken)
            throw new AliasConflictException("Label đã được sử dụng.");

        // Create alias using domain aggregate
        var alias = Models.Aliases.Alias.Create(
            label: command.Label,
            source: nicknameSource,
            visibility: AliasVisibility.Public,
            isSystemGenerated: false);

        dbContext.Aliases.Add(alias);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            // Publish integration event for cross-service communication
            var aliasIssuedEvent = new AliasIssuedIntegrationEvent(
                alias.Id,
                command.SubjectRef,
                alias.CurrentVersionId!.Value,
                alias.Label.Value,
                alias.CreatedAt);

            await publishEndpoint.Publish(aliasIssuedEvent, cancellationToken);
        }
        catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
        {
            throw new AliasConflictException("Label đã được sử dụng.", internalDetail: ex.Message);
        }

        var currentVersion = alias.CurrentVersion!;
        return new IssueAliasResult(
            alias.Id,
            currentVersion.Id,
            currentVersion.DisplayName,
            alias.Visibility.ToString(),
            alias.CreatedAt);
    }

    private NicknameSource ExtractNicknameSourceFromToken(string? reservationToken, string label)
    {
        var nicknameSource = NicknameSource.Custom;
        if (reservationToken is not null)
        {
            if (!aliasTokenService.TryValidate(reservationToken, out var aliasUniqueKeyFromToken, out var expiresAt))
                throw new AliasTokenException(AliasTokenFaultReason.Invalid);

            var uniqueKey = AliasNormalizerUtils.ToUniqueKey(label);
            if (aliasUniqueKeyFromToken != uniqueKey)
                throw new AliasTokenException(AliasTokenFaultReason.Mismatch, expiresAt);

            if (expiresAt < DateTimeOffset.UtcNow)
                throw new AliasTokenException(AliasTokenFaultReason.Expired, expiresAt);

            nicknameSource = NicknameSource.Gacha;
        }

        return nicknameSource;
    }
}