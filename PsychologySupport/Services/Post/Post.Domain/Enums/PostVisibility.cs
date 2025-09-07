using System.Text.Json.Serialization;

namespace Post.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostVisibility
{
    Public = 0,
    Draft = 1,
    Private = 2
}