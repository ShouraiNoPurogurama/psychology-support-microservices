namespace BuildingBlocks.Exceptions;

public class RateLimitExceededException : Exception
{
    public string? Details { get; set; }
    public RateLimitExceededException(string message = "Bạn đã thực hiện quá nhiều yêu cầu, vui lòng thử lại sau.") 
        : base(message) { }

    public RateLimitExceededException(string? details, string message) : base(message)
    {
        Details = details;
    }
}
