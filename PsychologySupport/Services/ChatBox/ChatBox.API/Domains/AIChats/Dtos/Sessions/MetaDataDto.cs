using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Sessions;

sealed record MetaDataDto(
    [property: JsonProperty("emotionContext")]
    string? EmotionContext,
    [property: JsonProperty("topic")]
    string? Topic,
    [property: JsonProperty("imageContext")]
    string? ImageContext
);