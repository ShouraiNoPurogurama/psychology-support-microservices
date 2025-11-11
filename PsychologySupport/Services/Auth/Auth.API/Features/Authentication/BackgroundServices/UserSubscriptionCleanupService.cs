using Auth.API.Data;

namespace Auth.API.Features.Authentication.BackgroundServices
{
    public class UserSubscriptionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserSubscriptionCleanupService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public UserSubscriptionCleanupService(IServiceProvider serviceProvider, ILogger<UserSubscriptionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UserSubscriptionCleanupService is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                    var now = DateTimeOffset.UtcNow;

                    var usersToUpdate = await dbContext.Users
                        .Where(u => u.IsFreeTrialUsed && u.ValidTo < now && u.SubscriptionPlanName == "Free Trial")
                        .ToListAsync(stoppingToken);

                    if (usersToUpdate.Any())
                    {
                        foreach (var user in usersToUpdate)
                        {
                            user.SubscriptionPlanName = "Free Plan";
                            user.ValidFrom = now;
                            user.ValidTo = null;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Updated {Count} users' subscription to Free Plan.", usersToUpdate.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating expired free trials.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("UserSubscriptionCleanupService is stopping...");
        }
    }
}
