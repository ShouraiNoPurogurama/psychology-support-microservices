using ChatBox.API.Domains.AIChats.Enums;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public sealed class MemoryToSaveDto
{
    [JsonProperty("summary")]
    public string Summary { get; set; } = "";

    // // raw tags do Router sinh ra (snake_case / kebab đều được, chuẩn hóa sau)
    // [JsonProperty("tags")]
    // public List<string> Tags { get; set; } = new();

    [JsonProperty("emotion_tags")]
    public List<EmotionTag>? EmotionTags { get; set; }

    [JsonProperty("relationship_tags")]
    public List<RelationshipTag>? RelationshipTags { get; set; }

    [JsonProperty("topic_tags")]
    public List<TopicTag>? TopicTags { get; set; }
}