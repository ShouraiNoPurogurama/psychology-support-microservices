using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Test.Application.Extensions.Utils;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services.Pdf;

public class Dass21PdfDocument(
    string clientName,
    DateTime testDate,
    int age,
    Score depression,
    Score anxiety,
    Score stress,
    SeverityLevel severityLevel,
    string completeTime,
    Recommendation recommendation,
    Dass21PercentileLookup lookup
    )
    : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        var depressionScore = depression.Value;
        var anxietyScore = anxiety.Value;
        var stressScore = stress.Value;
        
        var depressionDescriptor = Score.GetDepressionDescriptor(depressionScore);
        var anxietyDescriptor = Score.GetAnxietyDescriptor(anxietyScore);
        var stressDescriptor = Score.GetStressDescriptor(stressScore);
        
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Lookup", "Dass21Percentiles.csv");

        var depressionPercentile = lookup.GetPercentile("depression", depressionScore);
        var anxietyPercentile = lookup.GetPercentile("anxiety", anxietyScore);
        var stressPercentile = lookup.GetPercentile("stress", stressScore);
        var totalPercentile = lookup.GetPercentile("total", depressionScore + anxietyScore + stressScore);
        
        var avgPercentile = 
        
        container.Page(page =>
        {
            page.Margin(50);
            page.Size(PageSizes.A4);

            // Header with improved styling - Purple theme
            page.Header().Column(headerCol =>
            {
                headerCol.Item().PaddingBottom(20).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Báo cáo Sức khỏe Cảm xúc DASS-21")
                            .FontSize(24).Bold().FontColor(Colors.Purple.Darken2);
                        col.Item().PaddingTop(5).Text("Báo Cáo Kết Quả Đánh Giá Tâm Lý")
                            .FontSize(14).FontColor(Colors.Purple.Darken1);
                        col.Item().PaddingTop(2).Text("Hành trình chăm sóc sức khỏe tinh thần cùng EmoEase")
                            .FontSize(11).FontColor(Colors.Grey.Darken1);
                    });
                    
                    // Date box with border - Purple theme
                    row.ConstantItem(140).Border(1).BorderColor(Colors.Purple.Lighten2)
                        .Background(Colors.Purple.Lighten4).Padding(8).AlignCenter().Column(dateCol =>
                    {
                        dateCol.Item().Text("Ngày đánh giá").FontSize(10).FontColor(Colors.Grey.Darken2);
                        dateCol.Item().Text($"{testDate:dd/MM/yyyy}").FontSize(14).Bold().FontColor(Colors.Purple.Darken2);
                    });
                });
                
                headerCol.Item().PaddingTop(10).LineHorizontal(3).LineColor(Colors.Purple.Darken1);
            });

            // Content
            page.Content().Column(col =>
            {
                // Client Information Section
                col.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(15).Column(infoCol =>
                {
                    infoCol.Item().Text("THÔNG TIN KHÁCH HÀNG").FontSize(14).Bold().FontColor(Colors.Purple.Darken2);
                    infoCol.Item().PaddingTop(8).Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Item().Row(row =>
                        {
                            row.ConstantItem(80).Text("Họ và tên:").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text(clientName).FontSize(12).FontColor(Colors.Black);
                        });
                        grid.Item().Row(row =>
                        {
                            row.ConstantItem(40).Text("Tuổi:").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text($"{age}").FontSize(12).FontColor(Colors.Black);
                        });
                        grid.Item().Row(row =>
                        {
                            row.ConstantItem(130).Text("Thời gian hoàn thành:").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().Text(completeTime).FontSize(12).FontColor(Colors.Black);
                        });
                        grid.Item().Text("Đánh giá tự động từ EmoEase").FontSize(12).FontColor(Colors.Grey.Darken1);
                    });
                });

                // Results Section
                col.Item().PaddingTop(25).Text("KẾT QUẢ ĐÁNH GIÁ").FontSize(18).Bold().FontColor(Colors.Purple.Darken2);
                
                // Results Table with improved styling - Purple theme
                col.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2.5f);
                    });

                    // Table header - Purple theme
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Purple.Darken2).Padding(8)
                            .Text("Chỉ số").Bold().FontSize(12).FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken2).Padding(8)
                            .Text("Điểm").Bold().FontSize(12).FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken2).Padding(8)
                            .Text("Mức độ").Bold().FontSize(12).FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken2).Padding(8)
                            .Text("Phần trăm cộng đồng").Bold().FontSize(12).FontColor(Colors.White);
                    });

                    void AddResultRow(string scale, int score, string descriptor, string percentile, string color, bool isTotal = false)
                    {
                        var bgColor = isTotal ? Colors.Purple.Lighten4 : Colors.White;
                        var textStyle = isTotal ? TextStyle.Default.Bold() : TextStyle.Default;
                        
                        table.Cell().Background(bgColor).Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Text(scale).FontSize(11).Style(textStyle);
                        table.Cell().Background(bgColor).Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Text($"{score}").FontSize(11).Style(textStyle);
                        table.Cell().Background(bgColor).Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Text(descriptor).FontSize(11).Style(textStyle).FontColor(color);
                        table.Cell().Background(bgColor).Padding(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Text(percentile).FontSize(11).Style(textStyle);
                    }

                    var depressionTextColor = QuestPDFUtils.GetDescriptorColor(depressionDescriptor);
                    var anxietyTextColor = QuestPDFUtils.GetDescriptorColor(anxietyDescriptor);
                    var stressTextColor = QuestPDFUtils.GetDescriptorColor(stressDescriptor);
                    var severityLevelColor = QuestPDFUtils.GetSeverityLevelColor(severityLevel);
                    var vietnameseSeverityLevel = severityLevel.ToVietnamese();
                    
                    AddResultRow("Trầm cảm (Depression)", depressionScore, depressionDescriptor, depressionPercentile.ToString(), depressionTextColor);
                    AddResultRow("Lo âu (Anxiety)", anxietyScore, anxietyDescriptor, anxietyPercentile.ToString(), anxietyTextColor);
                    AddResultRow("Căng thẳng (Stress)", stressScore, stressDescriptor, stressPercentile.ToString(), stressTextColor);
                    AddResultRow("TỔNG ĐIỂM", depressionScore + anxietyScore + stressScore, 
                        vietnameseSeverityLevel, totalPercentile.ToString(), severityLevelColor, true);
                });

                // Chart Section
                col.Item().PaddingTop(20).Text("BIỂU ĐỒ KẾT QUẢ").FontSize(16).Bold().FontColor(Colors.Purple.Darken2);
                col.Item().PaddingTop(10).Element(CreateImprovedBarChart);

                // Interpretation Section
                col.Item().PaddingTop(25).Text("PHÂN TÍCH & GỢI Ý CHUYÊN MÔN").FontSize(16).Bold().FontColor(Colors.Purple.Darken2);
                
                // Overview box - Purple theme
                col.Item().PaddingTop(10).Border(1).BorderColor(Colors.Purple.Lighten2)
                    .Background(Colors.Purple.Lighten5).Padding(15).Column(overviewCol =>
                {
                    overviewCol.Item().Text("Tóm tắt kết quả:")
                        .FontSize(12).Bold().FontColor(Colors.Purple.Darken2);
                    overviewCol.Item().PaddingTop(5).Text(recommendation.Overview)
                        .FontSize(11).LineHeight(1.4f);
                });

                // Emotion Analysis
                col.Item().PaddingTop(15).Text("Nhận diện cảm xúc:")
                    .FontSize(12).Bold().FontColor(Colors.Purple.Darken2);
                col.Item().PaddingTop(5).Text(recommendation.EmotionAnalysis)
                    .FontSize(11).LineHeight(1.4f);

                // Suggestions
                col.Item().PaddingTop(20).Text("GỢI Ý CÁ NHÂN HÓA")
                    .FontSize(16).Bold().FontColor(Colors.Purple.Darken2);
                
                foreach (var suggestion in recommendation.PersonalizedSuggestions)
                {
                    col.Item().PaddingTop(18).Border(2).BorderColor(Colors.Purple.Lighten3)
                        .Background(Colors.Purple.Lighten5).Padding(16).Column(suggestionCol =>
                    {
                        // Title with icon-like bullet
                        suggestionCol.Item().Text($"▶ {suggestion.Title}")
                            .FontSize(13).Bold().FontColor(Colors.Purple.Darken1);
                        
                        // Description
                        suggestionCol.Item().PaddingTop(8).Text(suggestion.Description)
                            .FontSize(11).LineHeight(1.4f).FontColor(Colors.Grey.Darken3);
                        
                        // Tips section with improved styling
                        if (suggestion.Tips?.Any() == true)
                        {
                            suggestionCol.Item().PaddingTop(12).Text("Các bước thực hiện:")
                                .FontSize(11).Bold().FontColor(Colors.Purple.Darken2);
                            
                            // Tips container with light background
                            suggestionCol.Item().PaddingTop(6).Background(Colors.White)
                                .Border(1).BorderColor(Colors.Purple.Lighten2).Padding(10).Column(tipsCol =>
                            {
                                foreach (var tip in suggestion.Tips)
                                {
                                    tipsCol.Item().PaddingBottom(6).Row(tipRow =>
                                    {
                                        tipRow.ConstantItem(20).Text("✓")
                                            .FontSize(11).Bold().FontColor(Colors.Green.Darken1);
                                        tipRow.RelativeItem().Text(tip)
                                            .FontSize(10).LineHeight(1.3f).FontColor(Colors.Grey.Darken2);
                                    });
                                }
                            });
                        }
                        
                        // Reference with improved styling
                        suggestionCol.Item().PaddingTop(10).Background(Colors.Grey.Lighten4)
                            .Padding(8).Text($"📚 Tham khảo: {suggestion.Reference}")
                            .FontSize(9).Italic().FontColor(Colors.Grey.Darken3);
                    });
                }

                // Closing message - Purple theme
                col.Item().PaddingTop(20).Border(2).BorderColor(Colors.Purple.Lighten2)
                    .Background(Colors.Purple.Lighten5).Padding(15).Column(closingCol =>
                {
                    closingCol.Item().Text("Lời nhắn từ EmoEase")
                        .FontSize(12).Bold().FontColor(Colors.Purple.Darken2);
                    closingCol.Item().PaddingTop(5).Text(recommendation.Closing)
                        .FontSize(11).Italic().LineHeight(1.3f);
                });

                // Reference section
                col.Item().PaddingTop(25).LineHorizontal(2).LineColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(15).Text("THÔNG TIN THAM KHẢO")
                    .FontSize(12).Bold().FontColor(Colors.Purple.Darken2);

                col.Item().PaddingTop(8).Text(
                    "Báo cáo này dựa trên bộ thang đo DASS-21 được phát triển bởi Lovibond & Lovibond (1995) và chuẩn hóa bởi Henry & Crawford (2005). " +
                    "Kết quả chỉ mang tính chất tham khảo và không thay thế cho việc tư vấn hoặc chẩn đoán chuyên môn từ bác sĩ tâm lý. " +
                    "Nếu bạn cần hỗ trợ thêm, hãy liên hệ với chuyên gia tâm lý hoặc đội ngũ EmoEase."
                ).FontSize(10).FontColor(Colors.Grey.Darken2).LineHeight(1.3f);

                // Important notice - Keeping the warm orange color for the notice box
                col.Item().PaddingTop(10).Background(Colors.Yellow.Lighten4).Padding(10).Column(noticeCol =>
                {
                    noticeCol.Item().Text(text =>
                    {
                        text.Span("💡 Lưu ý nhỏ từ ").FontSize(10).Bold().FontColor(Colors.Orange.Darken2);
                        text.Span("EmoEase").FontSize(10).Bold().FontColor(Colors.Orange.Darken4);
                    });

                    noticeCol.Item()
                        .Text(text =>
                        {
                            text.Span("Nếu bạn cảm thấy cần chia sẻ thêm hoặc muốn tâm sự về cảm xúc của mình, ")
                                .FontSize(10)
                                .FontColor(Colors.Orange.Darken2);
                            text.Span("đừng ngần ngại trò chuyện với ")
                                .FontSize(10)
                                .FontColor(Colors.Orange.Darken2);
                            text.Span("Emo").FontSize(10).LineHeight(1.2f).SemiBold().FontColor(Colors.Orange.Darken3);
                            text.Span(" trên ứng dụng hoặc website ")
                                .FontSize(10)
                                .FontColor(Colors.Orange.Darken2);
                            text.Span("EmoEase").FontSize(10).SemiBold().LineHeight(1.2f).FontColor(Colors.Orange.Darken3);
                            text.Span(" nhé! Bạn luôn có một người bạn đồng hành lắng nghe 24/7.")
                                .FontSize(10)
                                .LineHeight(1.2f)
                                .FontColor(Colors.Orange.Darken3);
                        });
                });

            });

            // Footer with improved styling
            page.Footer().Column(footerCol =>
            {
                footerCol.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                footerCol.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Được tạo bởi EmoEase | Đánh giá DASS-21")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text($"Trang 1 | © 2025")
                        .FontSize(9).FontColor(Colors.Grey.Darken2).AlignRight();
                });
            });
        });
    }

    private IContainer CreateImprovedBarChart(IContainer container)
    {
        var data = new[] {
            ("Trầm cảm", depression.Value, Colors.Red.Darken1),
            ("Lo âu", anxiety.Value, Colors.Orange.Darken1),
            ("Căng thẳng", stress.Value, Colors.Purple.Darken1) // Changed to purple for consistency
        };

        float maxScore = 21f;

        container
            .Border(2)
            .BorderColor(Colors.Grey.Lighten1)
            .Background(Colors.White)
            .Padding(20)
            .Height(280)
            .Svg(size =>
            {
                float chartHeight = size.Height - 80;
                float chartWidth = size.Width - 80;
                float barWidth = Math.Min(60f, (chartWidth / data.Length) * 0.5f);
                float spacing = chartWidth / data.Length;
                float startX = 40;
                float startY = 20;

                var svgContent = "";

                // Title - Purple theme
                svgContent += $"""
                    <text x="{size.Width / 2}" y="25" font-size="14" font-weight="bold" text-anchor="middle" fill="{Colors.Purple.Darken2}">
                        Biểu đồ so sánh điểm số DASS-21
                    </text>
                """;

                // Y-axis with improved styling
                for (int i = 0; i <= 4; i++)
                {
                    float yValue = i * 5;
                    float yPos = startY + 40 + chartHeight - (yValue / maxScore * chartHeight);
                    
                    // Grid lines
                    svgContent += $"""
                        <line x1="{startX}" y1="{yPos}" x2="{startX + chartWidth}" y2="{yPos}" 
                              stroke="{Colors.Grey.Lighten2}" stroke-width="1" stroke-dasharray="3,3" />
                        <text x="{startX - 8}" y="{yPos + 4}" font-size="10" text-anchor="end" fill="{Colors.Grey.Darken2}">
                            {yValue}
                        </text>
                    """;
                }

                // Y-axis label
                svgContent += $"""
                    <text x="15" y="{startY + 40 + chartHeight / 2}" font-size="11" text-anchor="middle" 
                          fill="{Colors.Grey.Darken2}" transform="rotate(-90, 15, {startY + 40 + chartHeight / 2})">
                        Điểm số
                    </text>
                """;

                // X-axis
                svgContent += $"""
                    <line x1="{startX}" y1="{startY + 40 + chartHeight}" x2="{startX + chartWidth}" y2="{startY + 40 + chartHeight}" 
                          stroke="{Colors.Grey.Darken2}" stroke-width="2" />
                """;

                // Bars with improved styling
                for (int i = 0; i < data.Length; i++)
                {
                    var (label, value, color) = data[i];
                    float scaledHeight = (value / maxScore) * chartHeight;
                    float x = startX + (i * spacing) + (spacing - barWidth) / 2;
                    float y = startY + 40 + chartHeight - scaledHeight;

                    // Shadow effect
                    svgContent += $"""
                        <rect x="{x + 2}" y="{y + 2}" width="{barWidth}" height="{scaledHeight}" 
                              fill="{Colors.Grey.Lighten1}" rx="4" ry="4" opacity="0.3"/>
                    """;

                    // Main bar
                    svgContent += $"""
                        <rect x="{x}" y="{y}" width="{barWidth}" height="{scaledHeight}" 
                              fill="{color}" rx="4" ry="4"/>
                    """;

                    //Value label on top of bar
                    svgContent += $"""
                        <text x="{x + barWidth / 2}" y="{y - 8}" font-size="12" font-weight="bold" 
                              text-anchor="middle" fill="{Colors.Grey.Darken3}">
                            {value}
                        </text>
                    """;

                    //X-axis labels
                    svgContent += $"""
                        <text x="{x + barWidth / 2}" y="{startY + 40 + chartHeight + 20}" 
                              font-size="11" text-anchor="middle" fill="{Colors.Grey.Darken3}">
                            {label}
                        </text>
                    """;
                }

                //Legend
                float legendY = size.Height - 40;
                svgContent += $"""
                    <text x="{size.Width / 2}" y="{legendY}" font-size="10" text-anchor="middle" fill="{Colors.Grey.Darken2}">
                        Thang đo: 0-21 điểm | Mức độ nghiêm trọng tăng theo điểm số
                    </text>
                """;

                return $"""
                    <svg width="{size.Width}" height="{size.Height}" xmlns="http://www.w3.org/2000/svg">
                        {svgContent}
                    </svg>
                """;
            });
        
        return container;
    }
}