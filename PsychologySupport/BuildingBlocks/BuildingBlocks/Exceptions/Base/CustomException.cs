namespace BuildingBlocks.Exceptions.Base;

public class CustomException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public string SafeMessage { get; }           //trả về client
    public string? InternalDetail { get; }       //chỉ log

    protected CustomException(string errorCode, int statusCode, string safeMessage, string? internalDetail = null, Exception? inner = null)
        : base(safeMessage, inner)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        SafeMessage = safeMessage;
        InternalDetail = internalDetail;
    }
}