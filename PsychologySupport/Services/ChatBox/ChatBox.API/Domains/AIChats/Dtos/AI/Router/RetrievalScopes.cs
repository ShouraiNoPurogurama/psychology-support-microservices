using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class RetrievalScopes
{
    [JsonProperty("personal_memory")]
    public bool PersonalMemory { get; set; }

    [JsonProperty("team_knowledge")]
    public bool TeamKnowledge { get; set; }
}