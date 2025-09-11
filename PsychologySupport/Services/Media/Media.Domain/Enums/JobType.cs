using System.Text.Json.Serialization;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobType
    {
        Ingest,
        Transcode,
        Thumbnail,
        Optimize,
        ModerationRequest
    }
}
