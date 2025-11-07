using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class RetrievalHints
{
    [JsonProperty("keywords")]
    public List<string>? Keywords { get; set; }
}