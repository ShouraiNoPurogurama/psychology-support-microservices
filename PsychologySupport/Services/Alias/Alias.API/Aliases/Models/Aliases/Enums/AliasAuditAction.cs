using System.Text.Json.Serialization;

namespace Alias.API.Aliases.Models.Aliases.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AliasAuditAction
    {
        None, 
        Created,
        LabelUpdated,
        VisibilityChanged,
        AvatarChanged,
        Suspended,
        Restored,
        Deleted
    }
}