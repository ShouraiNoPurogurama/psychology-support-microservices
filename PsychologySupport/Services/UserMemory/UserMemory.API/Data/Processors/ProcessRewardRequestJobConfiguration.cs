namespace UserMemory.API.Data.Processors;

public static class ProcessRewardRequestJobConfiguration
{
    public const string JobName = "ProcessRewardRequestJob";
    
    // Chạy mỗi 3 giây
    public const string CronExpression = "0/3 * * * * ?";
}