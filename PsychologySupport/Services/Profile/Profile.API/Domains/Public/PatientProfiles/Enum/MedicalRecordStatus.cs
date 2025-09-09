using System.Text.Json.Serialization;

namespace Profile.API.Domains.PatientProfiles.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MedicalRecordStatus
{
    Processing,
    Done
}