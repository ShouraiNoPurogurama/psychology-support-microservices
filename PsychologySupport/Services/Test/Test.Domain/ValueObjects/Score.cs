using System.ComponentModel.DataAnnotations.Schema;
using Test.Domain.Enums;

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

    [NotMapped]
    public int MultipliedValue => Value * 2;

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

    public static string GetDepressionDescriptor(int score)
    {
        return score switch
        {
            <= 4 => "Bình thường",
            <= 6 => "Nhẹ",
            <= 10 => "Vừa",
            <= 13 => "Nặng",
            _ => "Rất nặng"
        };
    }

    public static string GetAnxietyDescriptor(int score)
    {
        return score switch
        {
            <= 3 => "Bình thường",
            <= 5 => "Nhẹ",
            <= 7 => "Vừa",
            <= 9 => "Nặng",
            _ => "Rất nặng"
        };
    }

    public static string GetStressDescriptor(int score)
    {
        return score switch
        {
            <= 7 => "Bình thường",
            <= 9 => "Nhẹ",
            <= 12 => "Vừa",
            <= 16 => "Nặng",
            _ => "Rất nặng"
        };
    }


    public static SeverityLevel GetDepressionLevelRaw(int score)
    {
        return score switch
        {
            >= 14 => SeverityLevel.Extreme,
            >= 11 => SeverityLevel.Severe,
            >= 7 => SeverityLevel.Moderate,
            >= 5 => SeverityLevel.Mild,
            _ => SeverityLevel.Normal
        };
    }

    public static SeverityLevel GetAnxietyLevelRaw(int score)
    {
        return score switch
        {
            >= 10 => SeverityLevel.Extreme,
            >= 8 => SeverityLevel.Severe,
            >= 6 => SeverityLevel.Moderate,
            >= 4 => SeverityLevel.Mild,
            _ => SeverityLevel.Normal
        };
    }

    public static SeverityLevel GetStressLevelRaw(int score)
    {
        return score switch
        {
            >= 17 => SeverityLevel.Extreme,
            >= 13 => SeverityLevel.Severe,
            >= 10 => SeverityLevel.Moderate,
            >= 8 => SeverityLevel.Mild,
            _ => SeverityLevel.Normal
        };
    }
}