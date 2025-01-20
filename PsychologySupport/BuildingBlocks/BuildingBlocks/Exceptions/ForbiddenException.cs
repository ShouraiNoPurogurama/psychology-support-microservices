namespace BuildingBlocks.Exceptions;

public class ForbiddenException : Exception
{
    public string? Details { get; set; }

    public ForbiddenException(string message = "Bạn không được cấp quyền để truy cập vào trang này.") : base(message)
    {
    }

    public ForbiddenException(string? details, string message) : base(message)
    {
        Details = details;
    }
}