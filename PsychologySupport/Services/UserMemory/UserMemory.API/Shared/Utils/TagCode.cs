using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Shared.Utils;

public static class TagCode
{
    public static string ToCode(this TopicTag t) => t.ToString();          // "Topic_Food"
    public static string ToCode(this EmotionTag t) => t.ToString();        // "Emotion_Sad"
    public static string ToCode(this RelationshipTag t) => t.ToString();   // "Relationship_Friend"
}