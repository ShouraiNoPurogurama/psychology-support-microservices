using BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.BackgroundServices
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionExpirationService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1); 

        public SubscriptionExpirationService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionExpirationService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubscriptionExpirationService is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();

                    var now = DateTimeOffset.UtcNow;

                    var expiredSubs = await dbContext.UserSubscriptions
                        .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var sub in expiredSubs)
                    {
                        sub.Status = SubscriptionStatus.Expired;

                        _logger.LogInformation("Subscription {Id} expired for Patient {PatientId}", sub.Id, sub.PatientId);

                        // publish event 
                        await _publishEndpoint.Publish(new UserSubscriptionExpiredIntegrationEvent(sub.PatientId), stoppingToken);
                    }

                    if (expiredSubs.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Marked {Count} subscriptions as Expired.", expiredSubs.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking expired subscriptions.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("SubscriptionExpirationService is stopping...");
        }
    }
}
