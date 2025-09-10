namespace BuildingBlocks.Utils;

public class RuleChain<T>
{
    private readonly ValidationBuilder<T> _parentBuilder;
    private readonly bool _isRuleViolated;
    private string? _errorCode;
    
    internal RuleChain(ValidationBuilder<T> parentBuilder, bool isRuleViolated)
    {
        _parentBuilder = parentBuilder;
        _isRuleViolated = isRuleViolated;
    }
    
    /// <summary>
    /// Gán mã lỗi cho quy tắc nếu nó bị vi phạm.
    /// </summary>
    /// <returns>Chính nó, để có thể gọi .WithMessage() tiếp theo.</returns>
    public RuleChain<T> WithErrorCode(string errorCode)
    {
        if (_isRuleViolated)
        {
            _errorCode = errorCode;
        }
        
        return this;
    }

    /// <summary>
    /// Gán thông điệp lỗi và hoàn tất quy tắc.
    /// </summary>
    /// <returns>ValidationBuilder gốc để bắt đầu một quy tắc mới.</returns>
    public ValidationBuilder<T> WithMessage(string errorMessage)
    {
        if (_isRuleViolated && !string.IsNullOrEmpty(_errorCode))
        {
            _parentBuilder.AddError(_errorCode, errorMessage);
        }
        
        return _parentBuilder;
    }
}


public class RuleChain
{
    private readonly ValidationBuilder _parentBuilder;
    private readonly bool _isRuleViolated;
    private string? _errorCode;

    internal RuleChain(ValidationBuilder parentBuilder, bool isRuleViolated)
    {
        _parentBuilder = parentBuilder;
        _isRuleViolated = isRuleViolated;
    }

    public RuleChain WithErrorCode(string errorCode)
    {
        if (_isRuleViolated)
        {
            _errorCode = errorCode;
        }
        return this;
    }

    public ValidationBuilder WithMessage(string errorMessage)
    {
        if (_isRuleViolated && !string.IsNullOrEmpty(_errorCode))
        {
            _parentBuilder.AddError(_errorCode, errorMessage);
        }
        return _parentBuilder;
    }
}