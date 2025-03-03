using System.Text.Json.Serialization;

namespace Test.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OptionValue
{
    NotAtAll,
    SeveralDays,
    MoreThanHalfTheDays,
    NearlyEveryDay
}