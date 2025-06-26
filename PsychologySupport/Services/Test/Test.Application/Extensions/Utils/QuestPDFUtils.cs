using QuestPDF.Helpers;
using Test.Domain.Enums;

namespace Test.Application.Extensions.Utils;

public static class QuestPDFUtils
{
    public static string GetDescriptorColor(string descriptor)
    {
        // descriptor tiếng Việt hoặc Enum đều dùng được
        return descriptor switch
        {
            "Bình thường" => Colors.Green.Darken2,
            "Nhẹ" => Colors.Blue.Darken2,
            "Vừa" => Colors.Orange.Darken1,
            "Nặng" => Colors.Red.Darken2,
            "Rất nặng" => Colors.Red.Accent4,
            _ => Colors.Grey.Darken2
        };
    }

    public static string GetSeverityLevelColor(SeverityLevel level)
    {
        return level switch
        {
            SeverityLevel.Normal => Colors.Green.Darken2,
            SeverityLevel.Mild => Colors.Blue.Darken2,
            SeverityLevel.Moderate => Colors.Orange.Darken1,
            SeverityLevel.Severe => Colors.Red.Darken2,
            SeverityLevel.Extreme => Colors.Red.Accent4,
            _ => Colors.Grey.Darken2
        };
    }
}