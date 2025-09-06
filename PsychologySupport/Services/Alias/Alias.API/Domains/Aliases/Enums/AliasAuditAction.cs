namespace Alias.API.Domains.Aliases.Enums;

public enum AliasAuditAction
{
    Create = 1,
    Update = 2,
    Rename = 3,
    VisibilityUpdate = 4,
    Restore = 5,
    Suspend = 6,
    Unsuspend = 7
}

public static class AliasAuditActionExtensions
{
    public static string GetName(this AliasAuditAction action) => action switch
    {
        AliasAuditAction.Create => "Create",
        AliasAuditAction.Update => "Update",
        AliasAuditAction.Rename => "Rename",
        AliasAuditAction.VisibilityUpdate => "Visibility Update",
        AliasAuditAction.Restore => "Restore",
        AliasAuditAction.Suspend => "Suspend",
        AliasAuditAction.Unsuspend => "Unsuspend",
        _ => "Unknown"
    };
}