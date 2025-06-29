using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PreferenceLevel
    {
        Daily,
        Weekly,
        Occasionally,
        Like,
        Neutral,
        Dislike
    }
    
    public static class PreferenceLevelExtensions
    {
        public static string ToVietnamese(this PreferenceLevel level)
        {
            return level switch
            {
                PreferenceLevel.Daily => "Hằng ngày",
                PreferenceLevel.Weekly => "Hàng tuần",
                PreferenceLevel.Occasionally => "Thỉnh thoảng",
                PreferenceLevel.Like => "Thích",
                PreferenceLevel.Neutral => "Bình thường",
                PreferenceLevel.Dislike => "Không thích",
                _ => level.ToString()
            };
        }
    }
}