using System.Text.Json.Serialization;

namespace AIModeration.API.Shared.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ViolationCategory
{
    HateSpeech,
    Violence,
    SexualContent,
    Harassment,
    SelfHarm,
    SpamScam,
    IllegalActivities
}