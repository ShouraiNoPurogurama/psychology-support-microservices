using ChatBox.API.Domains.AIChats.Enums;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class MemoryPayload
{
    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("emotion_tags")]
    public List<EmotionTag>? EmotionTags { get; set; }

    [JsonProperty("relationship_tags")]
    public List<RelationshipTag>? RelationshipTags { get; set; }

    [JsonProperty("topic_tags")]
    public List<TopicTag>? TopicTags { get; set; }

    // Hợp nhất enum để publish/tag thống nhất (nếu router trả về)
    [JsonProperty("normalized_tags")]
    public List<string>? NormalizedTags { get; set; }
}