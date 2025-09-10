using System.Text.Json.Serialization;

namespace Profile.API.Domains.Public.PatientProfiles.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MedicalRecordStatus
{
    Processing,
    Done
}