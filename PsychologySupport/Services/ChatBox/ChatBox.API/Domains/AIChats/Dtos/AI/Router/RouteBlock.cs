using ChatBox.API.Domains.AIChats.Enums;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class RouteBlock
{
    [JsonProperty("intent")]
    public RouterIntent Intent { get; set; }
}