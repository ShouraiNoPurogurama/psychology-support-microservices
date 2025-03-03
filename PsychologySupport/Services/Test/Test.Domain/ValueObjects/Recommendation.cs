namespace Test.Domain.ValueObjects;

public record Recommendation
{
    public Recommendation(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Recommendation cannot be empty.");

        if (value.Length > 1000)
            throw new ArgumentException("Recommendation cannot exceed 1000 characters.");

        Value = value;
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}