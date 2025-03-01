using System.Text.Json.Serialization;

namespace Profile.API.PatientProfiles.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PersonalityTrait
{
    Introversion,
    Extroversion,
    Adaptability
}