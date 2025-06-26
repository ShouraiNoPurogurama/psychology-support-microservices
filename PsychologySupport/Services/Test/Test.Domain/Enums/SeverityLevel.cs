using System.Text.Json.Serialization;

namespace Test.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeverityLevel
{
    Normal,
    Mild,
    Moderate,
    Severe,
    Extreme
}

public static class SeverityLevelExtensions
{
    public static string ToVietnamese(this SeverityLevel level)
    {
        return level switch
        {
            SeverityLevel.Normal   => "Bình thường",
            SeverityLevel.Mild     => "Nhẹ",
            SeverityLevel.Moderate => "Vừa",
            SeverityLevel.Severe   => "Nặng",
            SeverityLevel.Extreme  => "Rất nặng",
            _                     => "Không xác định"
        };
    }
}