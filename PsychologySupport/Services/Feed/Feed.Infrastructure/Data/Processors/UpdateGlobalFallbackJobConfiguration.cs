namespace Feed.Infrastructure.Data.Processors;

/// <summary>
/// Configuration for the UpdateGlobalFallbackJob.
/// Register this with Quartz.NET or Hangfire to run hourly.
/// </summary>
/// <example>
/// Quartz.NET configuration:
/// <code>
/// services.AddQuartz(q =>
/// {
///     var jobKey = new JobKey(UpdateGlobalFallbackJobConfiguration.JobName);
///     q.AddJob&lt;UpdateGlobalFallbackJob&gt;(opts => opts.WithIdentity(jobKey));
///     q.AddTrigger(opts => opts
///         .ForJob(jobKey)
///         .WithIdentity($"{UpdateGlobalFallbackJobConfiguration.JobName}-trigger")
///         .WithCronSchedule(UpdateGlobalFallbackJobConfiguration.CronExpression));
/// });
/// </code>
/// 
/// Hangfire configuration:
/// <code>
/// RecurringJob.AddOrUpdate&lt;UpdateGlobalFallbackJob&gt;(
///     UpdateGlobalFallbackJobConfiguration.JobName,
///     job => job.ExecuteAsync(CancellationToken.None),
///     Cron.Hourly);
/// </code>
/// </example>
public static class UpdateGlobalFallbackJobConfiguration
{
    public const string JobName = "UpdateGlobalFallbackJob";
    public const string CronExpression = "0 0/5 * * * ?";
    public static readonly TimeSpan LookbackPeriod = TimeSpan.FromHours(72);
    public const int TopPostsLimit = 500;
    public const int BatchSize = 750;
    /// <summary>
    /// Kích thước của mỗi "cửa sổ" thời gian khi quét lùi (tính bằng ngày).
    /// Ví dụ: mỗi lần lặp sẽ quét 7 ngày gần nhất, sau đó 7 ngày trước đó,...
    /// </summary>
    public const int ScanWindowDays = 7;

    /// <summary>
    /// Giới hạn quét lùi tối đa để tránh vòng lặp vô tận (tính bằng ngày).
    /// Job sẽ dừng nếu quét quá 30 ngày mà vẫn chưa đủ bài.
    /// </summary>
    public const int MaxLookbackDays = 30;

    
    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    public static void Validate()
    {
        if (TopPostsLimit <= 0)
            throw new InvalidOperationException($"{nameof(TopPostsLimit)} must be greater than 0");
        
        if (LookbackPeriod.TotalHours <= 0)
            throw new InvalidOperationException($"{nameof(LookbackPeriod)} must be greater than 0");
        
        if (MaxLookbackDays <= 0)
            throw new InvalidOperationException($"{nameof(MaxLookbackDays)} must be greater than 0");
    }
}