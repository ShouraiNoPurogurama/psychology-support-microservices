using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Test.Application.Extensions.Utils;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services.Pdf;

public class Dass21PdfDocument(
    string clientName,
    DateTimeOffset testDate,
    int age,
    string profileNickname,
    string profileDescription,
    List<string> profileHighlights,
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

        var depressionPercentile = lookup.GetPercentile("depression", depressionScore);
        var anxietyPercentile = lookup.GetPercentile("anxiety", anxietyScore);
        var stressPercentile = lookup.GetPercentile("stress", stressScore);
        var totalPercentile = lookup.GetPercentile("total", depressionScore + anxietyScore + stressScore);

        container.Page(page =>
        {
            page.Margin(40);
            page.Size(PageSizes.A4);

            // Header chỉ hiển thị ở trang đầu tiên
            page.Header()
                .ShowOnce()
                .Column(headerCol =>
                {
                    headerCol.Item()
                        .PaddingBottom(25)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item()
                                        .Text($"Hồ Sơ Sức khỏe Cảm Xúc")
                                        .FontSize(30)
                                        .Bold()
                                        .FontColor("#1a365d");
                                    col.Item()
                                        .PaddingTop(5)
                                        .Text($"{clientName}")
                                        .FontSize(22)
                                        .FontColor("#1a365d")
                                        .SemiBold();
                                    col.Item()
                                        .PaddingTop(8)
                                        .Text("Báo cáo Đánh Giá Tâm Lý Cá Nhân DASS-21")
                                        .FontSize(12)
                                        .FontColor("#2d3748"); // Dark gray
                                    col.Item()
                                        .PaddingTop(5)
                                        .Text("Hành trình chăm sóc sức khỏe tinh thần cùng EmoEase")
                                        .FontSize(12)
                                        .FontColor("#718096"); // Medium gray
                                });

                            // Date box với thiết kế theo PDF mẫu
                            row.ConstantItem(160)
                                .Border(1)
                                .BorderColor("#e2e8f0")
                                .Background("#f7fafc")
                                .AlignCenter()
                                .Column(dateCol =>
                                {
                                    dateCol.Item()
                                        .PaddingTop(26)
                                        .Text("Ngày đánh giá")
                                        .AlignCenter()
                                        .FontSize(11)
                                        .FontColor("#718096");
                                    dateCol.Item()
                                        .PaddingTop(6)
                                        .Text($"{testDate.AddHours(7):dd/MM/yyyy}")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor("#1a365d");
                                });
                        });

                    headerCol.Item().PaddingTop(15).LineHorizontal(2).LineColor("#e2e8f0");
                });

            // Content
            page.Content()
                .Column(col =>
                {
                    // Client Information Section - Professional Layout, No Icons
                    col.Item()
                        .PaddingTop(20)
                        .Background("#f8fafc") // Mềm hơn, sáng hơn
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .CornerRadius(10)
                        .PaddingVertical(22)
                        .PaddingHorizontal(32)
                        .Column(infoCol =>
                        {
                            // Section title: uppercase + spacing + font weight vừa phải
                            infoCol.Item()
                                .Text("THÔNG TIN KHÁCH HÀNG")
                                .FontSize(13)
                                .Bold()
                                .FontColor("#1a365d")
                                .LetterSpacing(.1f);

                            infoCol.Item().PaddingTop(10);

                            // Table thông tin với dòng phân cách
                            infoCol.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(115); // Label width
                                        columns.RelativeColumn();
                                    });

                                    void InfoRow(string label, string value, bool isLast = false)
                                    {
                                        var bgColor = isLast ? "#f8fafc" : "#f1f5f9";
                                        table.Cell()
                                            .Background(bgColor)
                                            .PaddingVertical(9)
                                            .PaddingLeft(2)
                                            .PaddingRight(10)
                                            .Text(label)
                                            .FontSize(12)
                                            .SemiBold()
                                            .FontColor("#64748b"); // blue-gray
                                        table.Cell()
                                            .Background(bgColor)
                                            .PaddingVertical(9)
                                            .PaddingLeft(8)
                                            .Text(value)
                                            .FontSize(13)
                                            .FontColor("#1a202c")
                                            .LineHeight(1.3f);
                                    }

                                    InfoRow("Họ và tên:", clientName, isLast: false);
                                    InfoRow("Tuổi:", $"{age} tuổi", isLast: true);
                                    InfoRow("Thời gian hoàn thành:", completeTime, isLast: false);
                                });
                        });
                    
                    // Bước 0
                    col.Item()
                        .PaddingVertical(12)
                        .PaddingHorizontal(16)
                        .Background("#eef6ff")
                        .CornerRadius(4)
                        .Shadow(new BoxShadowStyle
                        {
                            Color   = "#CCCCCC",
                            Blur    = 2,
                            OffsetX = 0,
                            OffsetY = 2,
                            Spread  = 0
                        })
                        .MinHeight(38)
                        .Column(callout =>
                        {
                            callout.Item()
                                .AlignCenter()
                                .AlignMiddle()
                                .Row(row =>
                                {
                                    row.ConstantItem(24)
                                        .PaddingLeft(4)
                                        .PaddingTop(10)
                                        .Text("✨")      // icon mới cho Khởi đầu
                                        .FontFamily("Noto Color Emoji")
                                        .FontSize(14);

                                    row.RelativeItem()
                                        .PaddingRight(14)
                                        .AlignCenter()
                                        .PaddingTop(10)
                                        .Text(text =>
                                        {
                                            text.Span("Bước 0:  ")
                                                .Italic()
                                                .FontSize(11)
                                                .FontColor("#4A5568");
                                            text.Span("“Khởi đầu hành trình thấu hiểu bản thân với biệt danh đặc biệt của bạn.”")
                                                .Italic()
                                                .FontColor("#4A5568")
                                                .FontSize(11)
                                                .LineHeight(1.5f);
                                        });
                                });
                        });

                    // Profile Section với màu sắc mới
                    col.Item()
                        .Background("#f8fafc")
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .Padding(20)
                        .Column(profileCol =>
                        {
                            profileCol.Item()
                                .Row(profileRow =>
                                {
                                    // ICON: Đặt icon lớn ở trái, ví dụ icon emoji hoặc hình vẽ survivor
                                    profileRow.ConstantItem(80)
                                        .AlignCenter()
                                        .Column(iconCol =>
                                        {
                                            iconCol.Item()
                                                .Text("🛡️") 
                                                .FontFamily("Noto Color Emoji")
                                                .FontSize(50)
                                                .AlignCenter();
                                            // iconCol.Item().PaddingTop(8).Text(profileNickname)
                                            //     .FontSize(12).SemiBold().AlignCenter().FontColor("#38a169");
                                        });

                                    // Phần thông tin nhóm
                                    profileRow.RelativeItem()
                                        .PaddingLeft(10)
                                        .Column(profileInfoCol =>
                                        {
                                            // Box tiêu đề lớn
                                            profileInfoCol.Item()
                                                .Text($"\"{profileNickname.ToUpper()}\"")
                                                .FontSize(24)
                                                .Bold()
                                                .FontColor("#1a365d"); // Nổi bật
                                            profileInfoCol.Item()
                                                .PaddingTop(6)
                                                .Text(profileDescription)
                                                .FontSize(13)
                                                .Italic()
                                                .FontColor("#2d3748")
                                                .LineHeight(1.3f);

                                            // Có thể thêm mô tả ngắn nổi bật hoặc tagline
                                            profileInfoCol.Item()
                                                .PaddingTop(20)
                                                .PaddingBottom(5)
                                                .Text("Đặc điểm nổi bật:")
                                                .FontSize(12)
                                                .Bold()
                                                .FontColor("#3182ce");

                                            // Bullet points các điểm mạnh (có thể truyền từ backend hoặc hardcode)
                                            profileInfoCol.Item()
                                                .PaddingTop(5)
                                                .Column(bulletCol =>
                                                {
                                                    foreach (var highlight in profileHighlights)
                                                    { 
                                                        bulletCol.Item()
                                                            .Row(row =>
                                                            {
                                                                row.ConstantItem(18)
                                                                    .Text("ℹ️")
                                                                    .FontFamily("Noto Color Emoji")
                                                                    .FontSize(11);
                                                                row.RelativeItem()
                                                                    .PaddingBottom(5)
                                                                    .Text(highlight)
                                                                    .FontSize(11)
                                                                    .FontColor("#4a5568");
                                                            });
                                                    }
                                                });
                                        });
                                });
                        });

//-----------------------------------------------------------------------------------------------------------------------------------
                    col.Item().PageBreak(); // Tạo một trang mới từ đây
//-----------------------------------------------------------------------------------------------------------------------------------

                    col.Item()
                        .PaddingVertical(14)
                        .PaddingHorizontal(16)
                        .Background("#eef6ff")
                        .CornerRadius(4)
                        .Shadow(new BoxShadowStyle
                        {
                            Color   = "#CCCCCC",
                            Blur    = 2,
                            OffsetX = 0,
                            OffsetY = 2,
                            Spread  = 0
                        })
                        .MinHeight(38)
                        .Column(callout =>
                        {
                            callout.Item()
                                .Row(row =>
                                {
                                    // Icon “🔍” tượng trưng cho việc soi xét, nội quan
                                    // row.ConstantItem(24)
                                    //     .PaddingLeft(4)
                                    //     .PaddingTop(10)
                                    //     .Text("🔍")
                                    //     .FontSize(14);

                                    row.RelativeItem()
                                        .PaddingTop(10)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text(text =>
                                        {
                                            text.Span("Bước 1:  ")
                                                .Italic()
                                                .FontSize(12)
                                                .FontColor("#4A5568");
                                            text.Span("“Dừng lại và quan sát suy nghĩ, cảm xúc của bản thân.”")
                                                .Italic()
                                                .FontSize(12)
                                                .FontColor("#4A5568")
                                                .LineHeight(1.5f);
                                        });
                                });
                        });


                    col.Item()
                        .Text("PHÂN TÍCH CHUYÊN SÂU CHỈ SỐ CẢM XÚC")
                        .FontSize(20)
                        .Bold()
                        .FontColor("#1a365d")
                        .LetterSpacing(0.1f);

                    col.Item()
                        .PaddingTop(5)
                        .Text("Các chỉ số dưới đây phản ánh tình trạng cảm xúc hiện tại của bạn")
                        .FontSize(11)
                        .FontColor("#718096")
                        .LetterSpacing(.1f);

                    // Bảng kết quả chuyên nghiệp
                    col.Item()
                        .PaddingTop(10)
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .CornerRadius(10)
                        .Background("#f8fafc")
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Chỉ số cảm xúc
                                columns.RelativeColumn(1.5f); // Điểm số
                                columns.RelativeColumn(2.5f); // Mức độ hiện tại
                                columns.RelativeColumn(2.5f); // So với cộng đồng
                            });

                            // Header
                            table.Header(header =>
                            {
                                Action<string> HeaderCell = (string text) =>
                                {
                                    header.Cell()
                                        .Background("#1a365d")
                                        .PaddingVertical(12)
                                        .AlignCenter()
                                        .Text(text.ToUpper())
                                        .Bold()
                                        .FontSize(11)
                                        .FontColor(Colors.White);
                                };
                                HeaderCell("Chỉ số cảm xúc");
                                HeaderCell("Điểm số");
                                HeaderCell("Mức độ hiện tại");
                                HeaderCell("So với cộng đồng");
                            });

                            string[] rowColors = { "#ffffff", "#f1f5f9" };
                            int rowIdx = 0;

                            void AddResultRow(string scale, int score, string descriptor, string percentile, string color,
                                bool isTotal = false)
                            {
                                var bgColor = isTotal ? "#e6f4ff" : rowColors[rowIdx % 2];
                                var labelStyle = isTotal ? TextStyle.Default.Bold() : TextStyle.Default;
                                var valueStyle = isTotal ? TextStyle.Default.Bold() : TextStyle.Default.SemiBold();

                                table.Cell()
                                    .Background(bgColor)
                                    .PaddingVertical(10)
                                    .PaddingHorizontal(8)
                                    .BorderBottom(1)
                                    .BorderColor("#e2e8f0")
                                    .Text(scale)
                                    .FontSize(12)
                                    .Style(labelStyle);

                                table.Cell()
                                    .Background(bgColor)
                                    .PaddingVertical(10)
                                    .PaddingHorizontal(8)
                                    .BorderBottom(1)
                                    .BorderColor("#e2e8f0")
                                    .AlignCenter()
                                    .Text($"{score}")
                                    .FontSize(12)
                                    .Style(valueStyle)
                                    .FontColor("#2d3748");

                                table.Cell()
                                    .Background(bgColor)
                                    .PaddingVertical(10)
                                    .PaddingHorizontal(8)
                                    .BorderBottom(1)
                                    .BorderColor("#e2e8f0")
                                    .AlignCenter()
                                    .Text(descriptor)
                                    .FontSize(12)
                                    .Style(valueStyle)
                                    .FontColor(color);

                                table.Cell()
                                    .Background(bgColor)
                                    .PaddingVertical(10)
                                    .PaddingHorizontal(8)
                                    .BorderBottom(1)
                                    .BorderColor("#e2e8f0")
                                    .AlignCenter()
                                    .Text($"{percentile}%")
                                    .FontSize(12)
                                    .Style(valueStyle)
                                    .FontColor("#2d3748");

                                rowIdx++;
                            }

                            // Lấy màu qua utils, không hardcode
                            var depressionTextColor = QuestPDFUtils.GetDescriptorColor(depressionDescriptor);
                            var anxietyTextColor = QuestPDFUtils.GetDescriptorColor(anxietyDescriptor);
                            var stressTextColor = QuestPDFUtils.GetDescriptorColor(stressDescriptor);
                            var severityLevelColor = QuestPDFUtils.GetSeverityLevelColor(severityLevel);
                            var vietnameseSeverityLevel = severityLevel.ToVietnamese();

                            AddResultRow(
                                "Cảm xúc trầm lắng", depressionScore, depressionDescriptor,
                                depressionPercentile.ToString(), depressionTextColor
                            );
                            AddResultRow(
                                "Cảm xúc lo lắng", anxietyScore, anxietyDescriptor,
                                anxietyPercentile.ToString(), anxietyTextColor
                            );
                            AddResultRow(
                                "Cảm xúc căng thẳng", stressScore, stressDescriptor,
                                stressPercentile.ToString(), stressTextColor
                            );
                            AddResultRow(
                                "TỔNG ĐIỂM", depressionScore + anxietyScore + stressScore,
                                vietnameseSeverityLevel, totalPercentile.ToString(), severityLevelColor, true
                            );
                        });


                    // Disclaimer với màu mới
                    col.Item()
                        .PaddingTop(8)
                        .Text("*Đây là chỉ số tham khảo phản ánh tình trạng cảm xúc tạm thời, không phải chẩn đoán y khoa")
                        .FontSize(10)
                        .Italic()
                        .FontColor("#718096");

                    // Chart Section
                    col.Item().PaddingTop(25).Text("BIỂU ĐỒ TỔNG QUAN").FontSize(18).Bold().FontColor("#1a365d");
                    col.Item().PaddingTop(12).Element(CreateImprovedBarChart);

                    col.Item()
                        .PaddingVertical(14)
                        .PaddingHorizontal(16)
                        .Background("#eef6ff")
                        .CornerRadius(4)
                        .Shadow(new BoxShadowStyle
                        {
                            Color   = "#CCCCCC",
                            Blur    = 2,
                            OffsetX = 0,
                            OffsetY = 2,
                            Spread  = 0
                        })
                        .MinHeight(38)
                        .Column(callout =>
                        {
                            callout.Item()
                                .Row(row =>
                                {
                                    // row.ConstantItem(24)
                                    //     .PaddingLeft(4)
                                    //     .PaddingTop(10)
                                    //     .Text("👂")      // icon mới cho lắng nghe
                                    //     .FontSize(14);

                                    row.RelativeItem()
                                        .PaddingTop(10)
                                        .PaddingLeft(15)
                                        .Text(text =>
                                        {
                                            text.Span("Bước 2:  ")
                                                .Italic()
                                                .FontSize(11)
                                                .FontColor("#4A5568");
                                            text.Span("“Cảm xúc không phải để che giấu, mà để lắng nghe và thấu hiểu.”")
                                                .Italic()
                                                .FontSize(11)
                                                .FontColor("#4A5568")
                                                .LineHeight(1.5f);
                                        });
                                });
                        });

                    // Phân tích cá nhân hóa - Modern Card Style
                    col.Item()
                        .Text("PHÂN TÍCH CÁ NHÂN HÓA")
                        .FontSize(18)
                        .Bold()
                        .FontColor("#1a365d");

                    col.Item()
                        .PaddingTop(14)
                        .Background("#f8fafc")
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .CornerRadius(12)
                        .PaddingVertical(22)
                        .PaddingHorizontal(32)
                        // .Shadow() // Nếu QuestPDF của bạn hỗ trợ shadow
                        .Column(overviewCol =>
                        {
                            // Optional: block heading/stripe
                            overviewCol.Item()
                                .Background("#e0e7ef")
                                .CornerRadius(8)
                                .PaddingVertical(7)
                                .PaddingHorizontal(12)
                                .Text("TỔNG QUAN TÂM LÝ")
                                .FontSize(12)
                                .Bold()
                                .FontColor("#1a365d")
                                .LetterSpacing(.1f);

                            overviewCol.Item()
                                .PaddingTop(8)
                                .Text(recommendation.Overview)
                                .FontSize(13)
                                .LineHeight(1.7f)
                                .FontColor("#25324B");
                        });

                    col.Item()
                        .PaddingVertical(12)
                        .AlignCenter()
                        .LineHorizontal(1)
                        .LineColor("#e2e8f0");

                    // call out
                    col.Item()
                        .PaddingVertical(14)
                        .PaddingHorizontal(16)
                        .Background("#eef6ff")      // cam pastel nhạt
                        .CornerRadius(4)
                        .Shadow(new BoxShadowStyle
                        {
                            Color   = "#CCCCCC", // đen với 25% opacity
                            Blur    = 2,           // mờ vừa đủ
                            OffsetX = 0,           // không dịch ngang
                            OffsetY = 2,           // dịch dọc nhẹ
                            Spread  = 0            // không lan rộng thêm
                        })
                        .MinHeight(38) 
                        .Column(callout =>
                        {
                            // Icon nhỏ phía trước
                            callout.Item()
                                .Row(row =>
                                {
                                    // row.ConstantItem(24)
                                    //     .PaddingLeft(4)
                                    //     .PaddingTop(10)
                                    //     .Text("🌱")      // icon mới cho sự phát triển
                                    //     .FontSize(14);

                                    row.RelativeItem()
                                        .PaddingTop(10)
                                        .PaddingLeft(15)
                                        .Text(text =>
                                        {
                                            text.Span("Bước 3:  ")
                                                .Italic()
                                                .FontSize(12)
                                                .FontColor("#4A5568");
                                            text.Span("“Biết được cảm xúc của mình là bước đầu để thay đổi.”")
                                                .Italic()
                                                .FontSize(12)
                                                .FontColor("#4A5568")
                                                .LineHeight(1.5f);
                                        });
                                });
                        });


                    col.Item()
                        .Background("#f8fafc")
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .CornerRadius(12)
                        .PaddingVertical(22)
                        .PaddingHorizontal(32)
                        .Column(emotionCol =>
                        {
                            // Block heading
                            emotionCol.Item()
                                .Background("#e0e7ef")
                                .CornerRadius(8)
                                .PaddingVertical(7)
                                .PaddingHorizontal(12)
                                .Text("NHẬN DIỆN CẢM XÚC")
                                .FontSize(12)
                                .Bold()
                                .FontColor("#1a365d")
                                .LetterSpacing(.1f);

                            // Nội dung
                            emotionCol.Item()
                                .PaddingTop(8)
                                .Text(recommendation.EmotionAnalysis)
                                .FontSize(13)
                                .LineHeight(1.7f)
                                .FontColor("#25324B");
                        });

//------------------------------------------------------------------------------------------------------------------------------------
                    // col.Item().PageBreak(); // Tạo một trang mới từ đây
//------------------------------------------------------------------------------------------------------------------------------------

                    // Callout “Khép lại hành trình”
                    col.Item()
                        .PaddingVertical(14)
                        .PaddingHorizontal(16)
                        .Background("#eef6ff")
                        .CornerRadius(4)
                        .Shadow(new BoxShadowStyle
                        {
                            Color   = "#CCCCCC",
                            Blur    = 2,
                            OffsetX = 0,
                            OffsetY = 2,
                            Spread  = 0
                        })
                        .MinHeight(50)
                        .Column(callout =>
                        {
                            callout.Item()
                                .Row(row =>
                                {
                                    // row.ConstantItem(24)
                                    //     .PaddingLeft(4)
                                    //     .PaddingTop(10)
                                    //     .Text("💓")
                                    //     .FontSize(14);

                                    row.RelativeItem()
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .PaddingTop(8)
                                        .Text(text =>
                                        {
                                            text.Span("Khép lại hành trình: ")
                                                .Italic()
                                                .FontSize(11)
                                                .FontColor("#4a5568");

                                            text.Span("“Bắt đầu bằng những bước nhỏ—mỗi hành động đều góp phần xây dựng bản thân vững chãi hơn.”")
                                                .Italic()
                                                .FontSize(11)
                                                .FontColor("#4a5568")
                                                .LineHeight(1.5f);
                                        });
                                });
                        });


                    col.Item()
                        .Text("GỢI Ý CÁ NHÂN HÓA CHO BẠN")
                        .FontSize(18)
                        .Bold()
                        .FontColor("#1a365d");

                    foreach (var (suggestion, index) in recommendation.PersonalizedSuggestions.Select((s, i) => (s, i)))
                    {
                        //Title block: number + title, tách riêng
                        col.Item()
                            .PaddingTop(22)
                            .Row(row =>
                            {
                                // Chip số thứ tự (vuông hoặc tròn, màu brand)
                                row.ConstantItem(40)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Background("#3c8dbc") // Đổi màu brand nếu muốn
                                    .CornerRadius(8)
                                    .PaddingVertical(9)
                                    .PaddingHorizontal(9)
                                    .Text($"{index + 1}")
                                    .FontSize(19)
                                    .Bold()
                                    .FontColor(Colors.White);

                                // Title + sub-title
                                row.RelativeItem()
                                    .PaddingTop(2)
                                    .PaddingLeft(18)
                                    .Column(titleCol =>
                                    {
                                        titleCol.Item()
                                            .Text(suggestion.Title)
                                            .FontSize(18)
                                            .Bold()
                                            .FontColor("#1a365d");
                                        titleCol.Item()
                                            .Text("Recommendation")
                                            .FontSize(12)
                                            .FontColor("#90a0b7")
                                            .LineHeight(1.2f);
                                    });
                            });

                        // Recommendation card - tách biệt khỏi title
                        col.Item()
                            .PaddingTop(6)
                            .Border(1)
                            .BorderColor("#e2e8f0")
                            .CornerRadius(10)
                            .Background("#f8fafc")
                            .PaddingVertical(25)
                            .PaddingHorizontal(30)
                            .Column(cardCol =>
                            {
                                // Description
                                cardCol.Item()
                                    .Text(suggestion.Description)
                                    .FontSize(13.5f)
                                    .LineHeight(1.4f)
                                    .FontColor("#374151");

                                // Action Steps (Tips)
                                if (suggestion.Tips.Any() == true)
                                {
                                    cardCol.Item()
                                        .PaddingTop(18)
                                        .Text("Các bước thực hiện:")
                                        .FontSize(13)
                                        .Bold()
                                        .FontColor("#1a365d")
                                        .LineHeight(1.2f);

                                    cardCol.Item()
                                        .PaddingTop(7)
                                        .Column(tipsCol =>
                                        {
                                            foreach (var tip in suggestion.Tips)
                                            {
                                                tipsCol.Item()
                                                    .Row(tipRow =>
                                                    {
                                                        tipRow.ConstantItem(18)
                                                            .AlignTop()
                                                            .Text("•")
                                                            .FontSize(16)
                                                            .FontColor("#20b26c"); // Green dot
                                                        tipRow.RelativeItem()
                                                            .Text(tip)
                                                            .FontSize(12)
                                                            .FontColor("#4a5568")
                                                            .LineHeight(1.6f);
                                                    });
                                            }
                                        });
                                }

                                // Reference cuối - stripe nhỏ
                                cardCol.Item()
                                    .PaddingTop(15)
                                    .Background("#f1f5f9")
                                    .CornerRadius(7)
                                    .PaddingVertical(8)
                                    .PaddingHorizontal(15)
                                    .Text(suggestion.Reference)
                                    .FontSize(11)
                                    .Italic()
                                    .FontColor("#6b7280");
                            });
                    }


//------------------------------------------------------------------------------------------------------------------------------------
                    col.Item().PageBreak(); // Chuyển đến Closing section
//------------------------------------------------------------------------------------------------------------------------------------

                    col.Item()
                        .PaddingVertical(12)
                        .Background("#fffaf0")
                        .CornerRadius(6)
                        .Padding(12)
                        .Text("“Emo tin rằng, chỉ cần một tia hy vọng, bạn đã có thể khám phá vô vàn tiềm năng bên trong.”")
                        .FontSize(11)
                        .Italic()
                        .FontColor("#6b7280");

                    col.Item()
                        .Background("#f8fafc")
                        .Border(1)
                        .BorderColor("#e2e8f0")
                        .CornerRadius(14)
                        .PaddingVertical(28)
                        .PaddingHorizontal(32)
                        .Column(closingCol =>
                        {
                            // Stripe heading thương hiệu
                            closingCol.Item()
                                .Background("#ffe4e6") // Hồng nhạt (warm, healing, nhẹ nhàng)
                                .CornerRadius(8)
                                .PaddingVertical(8)
                                .PaddingHorizontal(15)
                                .Text("LỜI NHẮN TỪ EMO")
                                .FontSize(12.5f)
                                .Bold()
                                .FontColor("#d72660")
                                ;
                            closingCol.Item()
                                .PaddingTop(10)
                                .Text(recommendation.Closing)
                                .FontSize(13)
                                .Italic()
                                .FontColor("#25324B")
                                .LineHeight(1.7f);

                            // (Có thể chèn block line nhỏ hoặc icon ở đây nếu muốn "signature")
                            closingCol.Item()
                                .PaddingTop(8)
                                .Text("— Emo 🌿")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor("#25324B")
                                .AlignRight();
                        });

                    // Reference và disclaimer
                    // Reference & Disclaimer section
                    col.Item()
                        .PaddingTop(30)
                        .LineHorizontal(1)
                        .LineColor("#e2e8f0");

                    col.Item()
                        .PaddingTop(17)
                        .Text("THÔNG TIN THAM KHẢO & LƯU Ý")
                        .FontSize(13.5f)
                        .Bold()
                        .FontColor("#1a365d");

                    col.Item()
                        .PaddingTop(10)
                        .Background("#f8fafc")
                        .CornerRadius(10)
                        .Padding(17)
                        .Text(
                            "Báo cáo này dựa trên bộ thang đo DASS-21 được phát triển bởi Lovibond & Lovibond (1995) và chuẩn hóa bởi Henry & Crawford (2005).\n" +
                            "Kết quả chỉ mang tính chất tham khảo về tình trạng cảm xúc hiện tại và không thay thế cho việc tư vấn hoặc chẩn đoán chuyên môn từ bác sĩ tâm lý.\n" +
                            "Nếu bạn cần hỗ trợ thêm, hãy liên hệ với chuyên gia tâm lý hoặc đội ngũ EmoEase."
                        )
                        .FontSize(11)
                        .FontColor("#718096")
                        .LineHeight(1.55f);

                    col.Item()
                        .PaddingTop(20)
                        .PaddingVertical(12)
                        .Background("#fffaf0")
                        .CornerRadius(6)
                        .Padding(12)
                        .Text("“Nếu trái tim cần sẻ chia, Emo luôn bên cạnh—bạn không phải đối mặt một mình.”")
                        .FontSize(11)
                        .Italic()
                        .FontColor("#6b7280");

                    // Notice cuối cùng - nổi bật, ấm áp, không dùng icon
                    col.Item()
                        .Background("#fff5f0")
                        .Border(1)
                        .BorderColor("#fed7cc")
                        .CornerRadius(10)
                        .PaddingVertical(20)
                        .PaddingHorizontal(22)
                        .Column(noticeCol =>
                        {
                            // Block heading nhẹ nhàng
                            noticeCol.Item()
                                .Background("#ffe4e6")
                                .CornerRadius(7)
                                .PaddingVertical(7)
                                .PaddingHorizontal(15)
                                .Text("LƯU Ý TỪ EMOEASE")
                                .FontSize(12)
                                .Bold()
                                .FontColor("#c53030");

                            noticeCol.Item()
                                .PaddingTop(8)
                                .Text(
                                    "Nếu bạn cảm thấy cần chia sẻ thêm hoặc muốn tâm sự về cảm xúc của mình, " +
                                    "đừng ngần ngại trò chuyện với Emo trên ứng dụng hoặc website EmoEase nhé. " +
                                    "Bạn luôn có một người bạn đồng hành lắng nghe 24/7."
                                )
                                .FontSize(11)
                                .FontColor("#a15b25")
                                .LineHeight(1.55f);
                        });
                });

            // Footer - chỉ hiển thị trên trang có nội dung
            page.Footer()
                .Column(footerCol =>
                {
                    footerCol.Item().PaddingTop(15).LineHorizontal(1).LineColor("#e2e8f0");
                    footerCol.Item()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Text("Được tạo bởi EmoEase | Đánh giá DASS-21")
                                .FontSize(10)
                                .FontColor("#718096");
                            row.RelativeItem()
                                .Text($"© 2025 | Tạo lúc: {DateTimeOffset.Now.AddHours(7):HH:mm dd/MM/yyyy}")
                                .FontSize(10)
                                .FontColor("#718096")
                                .AlignRight();
                        });
                });
        });
    }

    private IContainer CreateImprovedBarChart(IContainer container)
    {
        var data = new[]
        {
            ("Trầm cảm \\(Depression)", depression.Value, "#3182ce"),
            ("Lo âu \\(Anxiety)", anxiety.Value, "#dd6b20"),
            ("Căng thẳng \\(Stress)", stress.Value, "#e53e3e")
        };

        // Score chưa x2
        float maxScore = 21f;

        container
            .Border(1)
            .BorderColor("#e2e8f0")
            .Background("#f8fafc")
            .CornerRadius(10)
            .Padding(25)
            .Height(300)
            .Svg(size =>
            {
                float chartHeight = size.Height - 90;
                float chartWidth = size.Width - 100;
                float barWidth = Math.Min(70f, (chartWidth / data.Length) * 0.6f);
                float spacing = chartWidth / data.Length;
                float startX = 50;
                float startY = 30;

                var svgContent = "";

                // Title
                svgContent += $"""
                                   <text x="{size.Width / 2}" y="22" font-size="16" font-weight="bold" text-anchor="middle" fill="#1a365d">
                                       Biểu đồ tổng quan cảm xúc
                                   </text>
                               """;

                // Y-axis (tăng bước nhảy hợp lý: 7 = 42/6, hoặc chia theo nhu cầu)
                for (int i = 0; i <= 6; i++)
                {
                    float yValue = i * 7;
                    float yPos = startY + chartHeight - (yValue / maxScore * chartHeight);

                    svgContent += $"""
                                       <line x1="{startX}" y1="{yPos}" x2="{startX + chartWidth}" y2="{yPos}" 
                                             stroke="#e2e8f0" stroke-width="1" stroke-dasharray="2,2" />
                                       <text x="{startX - 12}" y="{yPos + 5}" font-size="11" font-family="Arial" text-anchor="end" fill="#7b8794">
                                           {yValue}
                                       </text>
                                   """;
                }

                // Y-axis label (đậm nét, hiện đại)
                svgContent += $"""
                                   <text x="15" y="{startY + chartHeight / 2}" font-size="12" font-weight="bold" text-anchor="middle" 
                                         fill="#718096" transform="rotate(-90, 15, {startY + chartHeight / 2})">
                                       Điểm số
                                   </text>
                               """;

                // X-axis
                svgContent += $"""
                                   <line x1="{startX}" y1="{startY + chartHeight}" x2="{startX + chartWidth}" y2="{startY + chartHeight}" 
                                         stroke="#2d3748" stroke-width="2" />
                               """;

                // Bars với hiệu ứng shadow, bo lớn, màu sáng
                for (int i = 0; i < data.Length; i++)
                {
                    var (label, value, color) = data[i];
                    float scaledHeight = (value / maxScore) * chartHeight;
                    float x = startX + (i * spacing) + (spacing - barWidth) / 2;
                    float y = startY + chartHeight - scaledHeight;

                    // Shadow effect
                    svgContent += $"""
                                       <rect x="{x + 3}" y="{y + 5}" width="{barWidth}" height="{scaledHeight}" 
                                             fill="#cbd5e0" rx="12" ry="12" opacity="0.18"/>
                                   """;

                    // Main bar
                    svgContent += $"""
                                       <rect x="{x}" y="{y}" width="{barWidth}" height="{scaledHeight}" 
                                             fill="{color}" rx="12" ry="12" opacity="0.92"/>
                                   """;

                    // Value label trên đầu bar (có dấu .0 thì loại đi)
                    svgContent += $"""
                                       <text x="{x + barWidth / 2}" y="{y - 10}" font-size="13" font-weight="bold" 
                                             text-anchor="middle" fill="#2d3748">
                                           {((value % 1 == 0) ? value.ToString("0") : value.ToString("0.##"))}
                                       </text>
                                   """;

                    // X-axis labels
                    var labelLines = label.Split('\\');
                    for (int j = 0; j < labelLines.Length; j++)
                    {
                        svgContent += $"""
                                           <text x="{x + barWidth / 2}" y="{startY + chartHeight + 22 + (j * 12)}" 
                                                 font-size="10" text-anchor="middle" fill="#475569">
                                               {labelLines[j]}
                                           </text>
                                       """;
                    }
                }

                // Legend (thang đo mới, update maxScore)
                float legendY = size.Height;
                svgContent += $"""
                                   <text x="{size.Width / 2}" y="{legendY}" font-size="11" text-anchor="middle" fill="#718096">
                                       Thang đo: 0-{maxScore} điểm | Điểm số cao hơn phản ánh cảm xúc khó khăn hơn
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