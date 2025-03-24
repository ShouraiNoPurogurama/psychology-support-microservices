using System.Text.Json.Serialization;

namespace Scheduling.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingStatus
    {
        AwaitPayment,
        AwaitMeeting,
        Completed,
        PaymentFailed,
        Cancelled
    }
    
    public static class BookingStatusExtensions
    {
        public static string ToReadableString(this BookingStatus status)
        {
            return status switch
            {
                BookingStatus.AwaitPayment => "Awaiting Payment",
                BookingStatus.AwaitMeeting => "Awaiting Meeting",
                BookingStatus.Completed => "Completed",
                BookingStatus.PaymentFailed => "Payment Failed",
                BookingStatus.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }
    }
}
