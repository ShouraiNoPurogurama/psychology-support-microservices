using BuildingBlocks.Exceptions;

namespace Post.Application.Exceptions;

public class TooManyRequestsException : BadRequestException
{
    public TooManyRequestsException(string? message = null) 
        : base(message ?? "Too many requests. Please try again later.")
    {
    }
}
