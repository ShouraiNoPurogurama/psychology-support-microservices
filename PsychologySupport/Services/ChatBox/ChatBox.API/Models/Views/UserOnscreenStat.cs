namespace ChatBox.API.Models.Views;

public class UserOnScreenStat
{
    public DateTime ActivityDate { get; set; }
    public long TotalActiveUsers { get; set; }
    public double TotalSystemOnscreenSeconds { get; set; }
    public double AvgOnscreenSecondsPerUser { get; set; }
}