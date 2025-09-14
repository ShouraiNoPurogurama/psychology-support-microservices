using System.Text.Json;
using Alias.API.Data.Public;
using Alias.API.Domains.Aliases.Enums;
using Alias.API.Domains.Aliases.Exceptions;
using Alias.API.Domains.Aliases.Utils;
using Alias.API.Models.Public;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;
using BuildingBlocks.Utils;
using FluentValidation;
using MassTransit;
using Pii.API.Protos;

namespace Alias.API.Domains.Aliases.Features.RenameAlias;

public record RenameAliasCommand(Guid SubjectRef, string Label) : ICommand<RenameAliasResult>;

public sealed class IssueAliasCommandValidator : AbstractValidator<RenameAliasCommand>
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

public record RenameAliasResult(string Label);

public class RenameAliasHandler(
    AliasDbContext dbContext,
    PiiService.PiiServiceClient piiClient,
    IPublishEndpoint publishEndpoint,
    ILogger<RenameAliasHandler> logger
) : ICommandHandler<RenameAliasCommand, RenameAliasResult>
{
    public async Task<RenameAliasResult> Handle(RenameAliasCommand command, CancellationToken ct)
    {
        var aliasIdResult = await piiClient.ResolveAliasIdBySubjectRefAsync(
            new ResolveAliasIdBySubjectRefRequest { SubjectRef = command.SubjectRef.ToString() },
            cancellationToken: ct);

        if (!Guid.TryParse(aliasIdResult.AliasId, out var aliasId))
            throw new NotFoundException("Thông tin Alias không hợp lệ.");

        if (aliasId == Guid.Empty)
            throw new AliasConflictException("Không tìm thấy hồ sơ người dùng để đổi tên.");

        var uniqueKey = AliasNormalizerUtils.ToUniqueKey(command.Label);
        var searchKey = AliasNormalizerUtils.ToSearchKey(command.Label);

        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        var labelTaken = dbContext.AliasVersions
            .AsNoTracking()
            .Any(v => v.UniqueKey == uniqueKey);

        if (labelTaken)
            throw new AliasConflictException("Label đã được sử dụng.");

        var alias = await dbContext.Aliases
                        .FirstOrDefaultAsync(a => a.Id == aliasId, ct)
                    ?? throw new AliasNotFoundException();

        var nicknameSource = NicknameSource.Custom;

        var newVersion = new AliasVersion
        {
            Id = Guid.NewGuid(),
            AliasId = alias.Id,
            Label = command.Label,
            SearchKey = searchKey,
            UniqueKey = uniqueKey,
            NicknameSource = nicknameSource,
            ValidFrom = DateTime.UtcNow
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
            
            await tx.CommitAsync(ct);
            
            var aliasUpdatedIntegrationEvent = new AliasUpdatedIntegrationEvent(
                alias.Id,
                command.SubjectRef,
                newVersion.Id,
                newVersion.Label,
                newVersion.ValidFrom);

            await publishEndpoint.Publish(aliasUpdatedIntegrationEvent, ct);
        }
        catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
        {
            throw new AliasConflictException("Label đã được sử dụng.", internalDetail: ex.Message);
        }

        return new RenameAliasResult(newVersion.Label);
    }
}