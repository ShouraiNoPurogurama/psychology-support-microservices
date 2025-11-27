namespace ChatBox.API.Models.Views;

public class WeeklyRetentionStat
{
    public long WeekNumber { get; set; } // Tuần thứ 1, 2, 3...
    public long EligibleUsers { get; set; } // Tổng user đủ thâm niên để đạt mốc này
    public long RetainedUsers { get; set; } // Số user thực sự quay lại
    public decimal RetentionRatePercent { get; set; } // Tỷ lệ %
}