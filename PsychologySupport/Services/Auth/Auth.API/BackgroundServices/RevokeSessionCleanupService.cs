using Auth.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.BackgroundServices
{
    public class RevokeSessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RevokeSessionCleanupService> _logger;
        //private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24); // chạy mỗi ngày
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(2);

        public RevokeSessionCleanupService(IServiceProvider serviceProvider, ILogger<RevokeSessionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RevokeSessionCleanupService is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                    var cutoff = DateTimeOffset.UtcNow.AddMonths(-1);
                    var oldSessions = await dbContext.DeviceSessions
                        .Where(s => s.IsRevoked && (s.RevokedAt < cutoff || s.LastRefeshToken < cutoff))
                        .ToListAsync(stoppingToken);

                    if (oldSessions.Any())
                    {
                        dbContext.DeviceSessions.RemoveRange(oldSessions);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Cleaned up {Count} revoked sessions older than 1 month.", oldSessions.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cleaning revoked sessions.");
                }

                await Task.Delay(CleanupInterval, stoppingToken);
            }

            _logger.LogInformation("RevokeSessionCleanupService is stopping...");
        }
    }
}
