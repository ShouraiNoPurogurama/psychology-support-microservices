using System.Text.Json.Serialization;

namespace Payment.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Pending,
    Completed,
    Failed
}