using Alias.API.Aliases.Exceptions;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Aliases.Utils;
using Alias.API.Common.Authentication;
using Alias.API.Common.Security;
using Alias.API.Data.Public;
using AIModeration.API.Protos;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Utils;
using MassTransit;
using Pii.API.Protos;

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
    PiiService.PiiServiceClient piiClient,
    ModerationService.ModerationServiceClient moderationClient,
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

        if (nicknameSource == NicknameSource.Custom)
        {
            var moderationRequest = new ValidateAliasLabelRequest { Label = command.Label };
            var moderationResult = await moderationClient.ValidateAliasLabelAsync(moderationRequest, cancellationToken: cancellationToken);
        
            if (!moderationResult.IsValid)
            {
                var reasons = string.Join(", ", moderationResult.Reasons);
                throw new BadRequestException($"Nickname không hợp lệ: {reasons}");
            }
        }

        var normalizedUniqueKey = AliasNormalizerUtils.ToUniqueKey(command.Label);
        
        var labelTaken = await dbContext.AliasVersions
            .AsNoTracking()
            .AnyAsync(v => v.UniqueKey == normalizedUniqueKey, cancellationToken);

        if (labelTaken)
            throw new AliasConflictException("Nickname đã được sử dụng.");

        // Create alias using domain aggregate
        var alias = Models.Aliases.Alias.Create(
            label: command.Label,
            source: nicknameSource,
            visibility: AliasVisibility.Public,
            isSystemGenerated: false);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        dbContext.Aliases.Add(alias);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            var subjectRef = currentActorAccessor.GetRequiredSubjectRef();

            var piiResponse = await piiClient.ResolveUserIdBySubjectRefAsync(
                new ResolveUserIdBySubjectRefRequest
                {
                    SubjectRef = subjectRef.ToString()
                },
                cancellationToken: cancellationToken);

            if (!Guid.TryParse(piiResponse.UserId, out var userId))
            {
                throw new UnauthorizedException("Yêu cầu không hợp lệ.", "SUBJECT_REF_NOT_FOUND");
            }

            // Publish integration event for cross-service communication
            var aliasIssuedEvent = new AliasIssuedIntegrationEvent(
                alias.Id,
                userId,
                command.SubjectRef,
                alias.CurrentVersionId!.Value,
                alias.Label.Value,
                alias.CreatedAt);

            await publishEndpoint.Publish(aliasIssuedEvent, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
        {
            throw new AliasConflictException("Nickname đã được sử dụng.", internalDetail: ex.Message);
        }
        catch (UnauthorizedException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
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