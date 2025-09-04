namespace BuildingBlocks.Messaging.Events.Queries.Scheduling;

public record ValidateBookingResponse(bool IsSuccess, List<string> Errors)
{
    public static ValidateBookingResponse Success() => new(true, new List<string>());
    public static ValidateBookingResponse Failed(string error) => new(false, new List<string> { error });
    public static ValidateBookingResponse Failed(List<string> errors) => new(false, errors);
}