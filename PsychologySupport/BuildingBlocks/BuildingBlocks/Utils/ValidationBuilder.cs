using BuildingBlocks.Exceptions;

namespace BuildingBlocks.Utils;

public class ValidationBuilder<T>
{
    private readonly T _instance;
    private readonly Dictionary<string, string[]> _errors = new();

    public ValidationBuilder(T instance)
    {
        _instance = instance;
    }

    public static ValidationBuilder<T> Create(T instance) => new ValidationBuilder<T>(instance);

    public RuleChain<T> When(Predicate<T> predicate)
    {
        bool isViolated = predicate(_instance);
        return new RuleChain<T>(this, isViolated);
    }

    internal void AddError(string errorCode, string errorMessage)
    {
        _errors.TryAdd(errorCode, [errorMessage]);
    }

    public void ThrowIfInvalid()
    {
        if (_errors.Count > 0)
        {
            throw new CustomValidationException(_errors);
        }
    }
}

public class ValidationBuilder
{
    private readonly Dictionary<string, string[]> _errors = new();

    private ValidationBuilder()
    {
    }

    public static ValidationBuilder Create() => new();

    public RuleChain When(Func<bool> condition)
    {
        bool isViolated = condition();
        return new RuleChain(this, isViolated);
    }

    internal void AddError(string errorCode, string errorMessage)
    {
        _errors.TryAdd(errorCode, [errorMessage]);
    }

    public void ThrowIfInvalid()
    {
        if (_errors.Count > 0)
        {
            throw new CustomValidationException(_errors);
        }
    }
}