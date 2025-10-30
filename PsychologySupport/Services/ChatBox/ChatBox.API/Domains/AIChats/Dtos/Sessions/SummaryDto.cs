using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Sessions;

sealed record SummaryDto(
    [property: JsonProperty("current")]
    string? Current,
    [property: JsonProperty("persist")]
    string? Persist,
    [property: JsonProperty("metadata")]
    MetaDataDto? Metadata,
    [property: JsonProperty("created_at")]
    DateTimeOffset? CreatedAt
);