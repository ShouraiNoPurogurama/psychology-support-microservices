using Alias.API.Aliases.Dtos;
using Alias.API.Aliases.Exceptions;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Common.Authentication;
using Alias.API.Data.Public;
using BuildingBlocks.CQRS;
using FluentValidation;

namespace Alias.API.Aliases.Features.UpdatePreferences;

public record UpdatePreferencesCommand(
    PreferenceTheme? Theme,
    PreferenceLanguage? Language,
    bool? NotificationsEnabled)
    : ICommand<UpdatePreferencesResult>;

public record UpdatePreferencesResult(
    UserPreferencesDto Preferences,
    DateTimeOffset UpdatedAt);

public sealed class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesCommandValidator()
    {
        RuleFor(x => x.Theme)
            .IsInEnum()
            .When(x => x.Theme.HasValue)
            .WithMessage("Theme must be a valid preference theme.");

        RuleFor(x => x.Language)
            .IsInEnum()
            .When(x => x.Language.HasValue)
            .WithMessage("Language must be a valid preference language.");

        RuleFor(x => x)
            .Must(x => x.Theme.HasValue || x.Language.HasValue || x.NotificationsEnabled.HasValue)
            .WithMessage("At least one preference field must be provided.");
    }
}

public class UpdatePreferencesHandler(
    AliasDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor)
    : ICommandHandler<UpdatePreferencesCommand, UpdatePreferencesResult>
{
    public async Task<UpdatePreferencesResult> Handle(UpdatePreferencesCommand command,
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
                    ?? throw new AliasNotFoundException("Không tìm thấy hồ sơ người dùng để cập nhật tùy chọn.",
                        "ALIAS_NOT_FOUND");

        // Use domain aggregate method to update preferences
        alias.UpdatePreferences(
            theme: command.Theme,
            language: command.Language,
            notificationsEnabled: command.NotificationsEnabled);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var preferencesDto = new UserPreferencesDto(
            Theme: alias.Preferences.Theme,
            Language: alias.Preferences.Language,
            NotificationsEnabled: alias.Preferences.NotificationsEnabled
        );

        return new UpdatePreferencesResult(
            preferencesDto,
            DateTimeOffset.UtcNow);
    }
}
