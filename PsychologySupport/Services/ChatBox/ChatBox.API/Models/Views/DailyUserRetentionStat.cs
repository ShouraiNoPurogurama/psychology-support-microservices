namespace ChatBox.API.Models.Views;

public class DailyUserRetentionStat
{
    public DateTime ActivityDate { get; set; }

    public long TotalActiveUsers { get; set; }

    public long NewUsers { get; set; }

    public long ReturningUsers { get; set; }

    public decimal TotalUsersToDate { get; set; }

    public decimal ReturningPercentage { get; set; }
}