namespace Media.Domain.Enums;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaPurpose
{
    /// <summary>
    /// File được upload để gắn vào một bài viết.
    /// </summary>
    PostAttachment,

    /// <summary>
    /// File được upload để làm ảnh đại diện.
    /// </summary>
    Avatar,

    /// <summary>
    /// File được upload để làm ảnh bìa profile.
    /// </summary>
    ProfileBackground,

    /// <summary>
    /// File được upload để gắn vào một bình luận.
    /// </summary>
    CommentAttachment
}