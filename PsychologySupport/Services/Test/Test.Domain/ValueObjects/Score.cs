namespace Test.Domain.ValueObjects;

public record Score
{
    public Score(int value)
    {
        if (value is < 0 or > 80)
            throw new ArgumentException("Score must be between 0 and 100.");

        Value = value;
    }

    public int Value { get; }

    public static Score Create(int value)
    {
        return new Score(value);
    }

    public static Score Zero()
    {
        return new Score(0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}