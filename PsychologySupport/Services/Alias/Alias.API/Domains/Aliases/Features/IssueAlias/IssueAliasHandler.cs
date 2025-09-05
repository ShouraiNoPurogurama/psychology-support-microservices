using System.Text.Json;
using Alias.API.Data.Public;
using Alias.API.Domains.Aliases.Common.Security;
using Alias.API.Domains.Aliases.Enums;
using Alias.API.Domains.Aliases.Exceptions;
using Alias.API.Domains.Aliases.Utils;
using Alias.API.Models.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using FluentValidation;
using MassTransit;
using Pii.API.Protos;

namespace Alias.API.Domains.Aliases.Features.IssueAlias;

public record IssueAliasCommand(Guid SubjectRef, string? ReservationToken, string Label)
    : ICommand<IssueAliasResult>;

public record IssueAliasResult(Guid AliasId, Guid AliasVersionId, string Label, string Visibility);

public sealed class IssueAliasCommandValidator : AbstractValidator<IssueAliasCommand>
{
    public IssueAliasCommandValidator()
    {
        RuleFor(x => x.SubjectRef)
            .NotEmpty()
            .WithMessage("Reference không được để trống.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Tên bí danh không được để trống.")
            .MaximumLength(30)
            .WithMessage("Tên bí danh tối đa 30 kí tự.");
    }
}

public class IssueAliasHandler(
    AliasDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IAliasTokenService aliasTokenService,
    PiiService.PiiServiceClient piiClient
) : ICommandHandler<IssueAliasCommand, IssueAliasResult>
{
    public async Task<IssueAliasResult> Handle(IssueAliasCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var aliasIdResult = await piiClient.ResolveAliasIdBySubjectRefAsync(
            new ResolveAliasIdBySubjectRefRequest { SubjectRef = command.SubjectRef.ToString() },
            cancellationToken: ct);

        if (!Guid.TryParse(aliasIdResult.AliasId, out var aliasId))
            throw new NotFoundException("Thông tin Alias không hợp lệ.");

        if (aliasId != Guid.Empty)
            throw new AliasConflictException("Người dùng đã có bí danh, không thể tạo mới.");
        
        var uniqueKey = AliasNormalizerUtils.ToUniqueKey(command.Label);
        var searchKey = AliasNormalizerUtils.ToSearchKey(command.Label);
        
        var nicknameSource = NicknameSource.Custom;

        if (command.ReservationToken is not null)
        {
            if (!aliasTokenService.TryValidate(command.ReservationToken, out var aliasUniqueKeyFromToken, out var expiresAt))
                throw new AliasTokenException(AliasTokenFaultReason.Invalid);

            if (aliasUniqueKeyFromToken != uniqueKey)
                throw new AliasTokenException(AliasTokenFaultReason.Mismatch, expiresAt);

            if (expiresAt < DateTimeOffset.UtcNow)
                throw new AliasTokenException(AliasTokenFaultReason.Expired, expiresAt);

            nicknameSource = NicknameSource.Gacha;
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        var labelTaken = dbContext.AliasVersions
            .AsNoTracking()
            .Any(v => v.UniqueKey == uniqueKey);

        if (labelTaken)
            throw new AliasConflictException("Label đã được sử dụng.");

        var alias = new Models.Public.Alias
        {
            Id = Guid.NewGuid(),
            AliasVisibility = AliasVisibility.Public
        };

        dbContext.Aliases.Add(alias);

        await dbContext.SaveChangesAsync(ct);

        var newVersion = new AliasVersion
        {
            Id = Guid.NewGuid(),
            AliasId = alias.Id,
            Label = command.Label,
            SearchKey = searchKey,
            UniqueKey = uniqueKey,
            NicknameSource = nicknameSource,
            ValidFrom = now
        };

        alias.CurrentVersionId = newVersion.Id;

        dbContext.AliasVersions.Add(newVersion);
        dbContext.AliasAudits.Add(new AliasAudit
        {
            Id = Guid.NewGuid(),
            AliasId = aliasId,
            Action = AliasAuditAction.Create.GetName(),
            Details = JsonSerializer.Serialize(new
            {
                label = command.Label,
                source = nicknameSource.ToString()
            })
        });

        try
        {
            await dbContext.SaveChangesAsync(ct);

            var aliasIssuedIntegrationEvent = new AliasIssuedIntegrationEvent(
                alias.Id,
                command.SubjectRef
            );
            await publishEndpoint.Publish(aliasIssuedIntegrationEvent, ct);
            
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new AliasConflictException("Label đã được sử dụng.", internalDetail: ex.Message);
        }

        //TODO: publish event nếu sau này cần dùng (AliasIssued)

        return new IssueAliasResult(alias.Id, newVersion.Id, newVersion.Label, alias.AliasVisibility.ToString());
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
}