using System.Text.Json.Serialization;
using BuildingBlocks.Utils;

namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIMessageResponseDto(
    Guid SessionId,
    bool SenderIsEmo,
    [property: JsonConverter(typeof(RawJsonStringConverter))] string Content,
    DateTimeOffset CreatedDate,
    CtaBlock? Cta = null
);