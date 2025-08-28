using Alias.API.Common.Security;
using Alias.API.Data.Public;
using Alias.API.Enums;
using Alias.API.Models.Public;
using Alias.API.Utils;
using BuildingBlocks.CQRS;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Features.UpsertAlias;

public record UpsertAliasCommand(
    Guid AliasId,
    string AliasLabel,
    string Token,
    NicknameSource NicknameSource) : ICommand<UpsertAliasResult>;

public record UpsertAliasResult(Guid AliasId);

public sealed class UpsertAliasCommandValidator : AbstractValidator<UpsertAliasCommand>
{
    public UpsertAliasCommandValidator()
    {
        RuleFor(x => x.AliasId)
            .NotEmpty().WithMessage("AliasId không được để trống.");

        RuleFor(x => x.AliasLabel)
            .NotEmpty().WithMessage("Label không được để trống.")
            .MaximumLength(20).WithMessage("Label tối đa 20 kí tự.");

        When(x => x.NicknameSource == NicknameSource.Gacha, () =>
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token không được để trống cho gacha mode.");
        });
    }
}

public class UpsertAliasHandler : ICommandHandler<UpsertAliasCommand, UpsertAliasResult>
{
    private readonly PublicDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAliasTokenService _aliasTokenService;

    public UpsertAliasHandler(PublicDbContext db, IPublishEndpoint publishEndpoint, IAliasTokenService aliasTokenService)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _aliasTokenService = aliasTokenService;
    }

    public async Task<UpsertAliasResult> Handle(UpsertAliasCommand command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var alias = await _db.Aliases
            .Include(a => a.AliasVersions)
            .FirstOrDefaultAsync(a => a.Id == command.AliasId, cancellationToken);

        var aliasKey = AliasNormalizer.ToKey(command.AliasLabel);
        
        if (command.NicknameSource == NicknameSource.Gacha)
        {
            if (!_aliasTokenService.TryValidate(command.Token, out var tokenAliasKey, out var expiresAt))
                throw new InvalidOperationException("Alias token không hợp lệ.");

            if (tokenAliasKey != aliasKey)
                throw new InvalidOperationException("Alias label không khớp với token.");

            if (expiresAt < DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Alias token đã hết hạn.");
        }
        
        if (alias is null)
        {
            //Tạo mới alias nếu chưa có
            var newVersion = new AliasVersion
            {
                Id = Guid.NewGuid(),
                AliasId = command.AliasId,
                AliasLabel = command.AliasLabel,
                AliasKey = aliasKey,
                NicknameSource = command.NicknameSource,
                ValidFrom = now,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = command.AliasId.ToString()
            };

            alias = new Models.Public.Alias
            {
                Id = command.AliasId,
                CurrentVersionId = newVersion.Id,
                AliasVersions = new List<AliasVersion> { newVersion }
            };

            await _db.Aliases.AddAsync(alias, cancellationToken);
        }

        else
        {
            //Nếu alias đã tồn tại → tạo bản mới
            var currentVersion = alias.AliasVersions.FirstOrDefault(x => x.ValidTo == null);
            if (currentVersion != null)
            {
                currentVersion.ValidTo = now;
            }

            var newVersion = new AliasVersion
            {
                Id = Guid.NewGuid(),
                AliasId = alias.Id,
                AliasLabel = command.AliasLabel,
                AliasKey = aliasKey,
                NicknameSource = command.NicknameSource,
                ValidFrom = now,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = command.AliasId.ToString()
            };

            alias.CurrentVersionId = newVersion.Id;
            alias.AliasVersions.Add(newVersion);
        }

        _db.AliasAudits.Add(new AliasAudit
        {
            Id = Guid.NewGuid(),
            AliasId = command.AliasId,
            Action = "UPD",
            Details = $"Label: {command.AliasLabel}, Source: {command.NicknameSource}",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = command.AliasId.ToString()
        });

        await _db.SaveChangesAsync(cancellationToken);
        
        //TODO publish 1 event để moderation service kiểm tra tính hợp lệ của tên nếu nickname_source = custom.
        
        // if (command.NicknameSource == NicknameSource.Custom)
        // {
        //     
        // }
        
        return new UpsertAliasResult(alias.Id);
    }
}