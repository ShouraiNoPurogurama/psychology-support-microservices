namespace BuildingBlocks.Exceptions;

public class CustomValidationException : BadRequestException
{ 
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public CustomValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(
            errorCode: "VALIDATION_ERROR",
            safeMessage: "Một hoặc nhiều lỗi xác thực đã xảy ra.")
    {
        Errors = errors;
    }
}