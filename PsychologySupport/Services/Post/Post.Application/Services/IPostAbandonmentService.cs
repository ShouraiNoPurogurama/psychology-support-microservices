using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Post.Application.Data;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Integration;
using Post.Domain.Aggregates.Posts.DomainEvents;

namespace Post.Application.Services;

public interface IPostAbandonmentService
{
    Task ScheduleAbandonmentCheckAsync(Guid postId, CancellationToken cancellationToken = default);
    Task CancelAbandonmentCheckAsync(Guid postId, CancellationToken cancellationToken = default);
}

public class PostAbandonmentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostAbandonmentBackgroundService> _logger;
    private readonly PostAbandonmentOptions _options;

    public PostAbandonmentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PostAbandonmentBackgroundService> logger,
        IOptions<PostAbandonmentOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAbandonedPosts(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking abandoned posts");
            }

            await Task.Delay(_options.CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAbandonedPosts(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IPostDbContext>();
        var outboxWriter = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

        var abandonmentThreshold = DateTimeOffset.UtcNow.Subtract(_options.AbandonmentThreshold);

        var abandonedPosts = await context.Posts
            .Where(p => !p.IsDeleted &&
                       p.CreatedAt < abandonmentThreshold &&
                       p.Metrics.ReactionCount == 0 &&
                       p.Metrics.CommentCount == 0 &&
                       !p.IsAbandonmentEventEmitted) // Add this flag to domain model
            .Take(100) // Process in batches
            .ToListAsync(cancellationToken);

        foreach (var post in abandonedPosts)
        {
            var abandonedEvent = new PostAbandonedEvent(
                post.Id,
                post.Author.AliasId,
                post.CreatedAt,
                DateTime.UtcNow
            );

            await outboxWriter.WriteAsync(abandonedEvent, cancellationToken);
            
            // Mark as processed to avoid duplicate events
            post.MarkAbandonmentEventEmitted();
        }

        if (abandonedPosts.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} abandoned posts", abandonedPosts.Count);
        }
    }
}

public class PostAbandonmentOptions
{
    public const string SectionName = "PostAbandonment";

    public TimeSpan AbandonmentThreshold { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(2);
}
