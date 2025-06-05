using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethodName
{
    VNPay,
    PayOS
}