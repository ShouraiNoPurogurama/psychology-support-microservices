namespace BuildingBlocks.Exceptions;

public class ForbiddenException : Exception
{
    public string? Details { get; set; }

    public ForbiddenException(string message = "You don't have permission to access this page.") : base(message)
    {
    }

    public ForbiddenException(string? details, string message) : base(message)
    {
        Details = details;
    }
}