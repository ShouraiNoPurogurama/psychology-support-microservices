using System.Timers;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Data;
using Timer = System.Timers.Timer;

namespace Promotion.Grpc.BackgroundServices;

public class UpdatePromotionStatusesBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<UpdatePromotionStatusesBackgroundService> logger
) : BackgroundService
{
    private Timer? _timer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("****** UpdatePromotionStatusesBackgroundService started.");
        ScheduleNextRun();
        return Task.CompletedTask;
    }

    private void ScheduleNextRun()
    {
        var now = DateTimeOffset.Now;
        var nextRun = now.Date.AddDays(1);
        var timeToNextRun = (nextRun - now).TotalMilliseconds;

        logger.LogInformation($"******Next run scheduled at: {nextRun}");
        
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
        
        _timer = new Timer(timeToNextRun);
        _timer.AutoReset = false;
        _timer.Elapsed += async (sender, args) => await InactivatePromotions();
        _timer.Start();
    }

    private async Task InactivatePromotions()
    {
        try
        {
            logger.LogInformation("******Inactivating expired promotions...");
             
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();

            var expiredPromotions = await dbContext.Promotions
                .Where(p => p.IsActive && p.EndDate <= DateTimeOffset.UtcNow)
                .Include(p => p.PromoCodes)
                .Include(p => p.GiftCodes)
                .ToListAsync();

            foreach (var promotion in expiredPromotions)
            {
                promotion.IsActive = false;
                foreach (var promoCode in promotion.PromoCodes) promoCode.IsActive = false;
                foreach (var giftCode in promotion.GiftCodes) giftCode.IsActive = false;
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation($"******Inactivated {expiredPromotions.Count} promotions.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "******Error occurred while inactivating promotions.");
        }
        finally
        {
            ScheduleNextRun();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
