using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Notification.API.Data.Processors;

public class OutboxListener(IOptions<AppSettings> appsettings, IServiceProvider serviceProvider, ILogger<OutboxListener> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = new NpgsqlConnection(appsettings.Value.ServiceDbContext.NotificationDb);
                await connection.OpenAsync(stoppingToken);

                await using var cmd = new NpgsqlCommand("LISTEN outbox_new_message", connection);
                await cmd.ExecuteNonQueryAsync(stoppingToken);

                connection.Notification += async (sender, e) =>
                {
                    try
                    {
                        var outboxId = Guid.Parse(e.Payload);

                        using var scope = serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(o => o.Id == outboxId, stoppingToken);
                        if (message == null) return;

                        var eventType = Type.GetType(message.Type);
                        var eventMessage = JsonSerializer.Deserialize(message.Content, eventType);

                        if (eventMessage is IDomainEvent domainEvent)
                        {
                            await mediator.Publish(domainEvent, stoppingToken);
                        }

                        message.ProcessedOn = DateTimeOffset.UtcNow.AddHours(7);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        //TODO: Log lỗi cho notify xử lý event
                    }
                };

                //Main loop: giữ connection alive, chờ Notify
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await connection.WaitAsync(stoppingToken); //Chỉ wait cho event Notification được đẩy lên
                    }
                    catch (OperationCanceledException)
                    {
                        // Đang shutdown, thoát loop
                        break;
                    }
                    catch (Exception ex)
                    {
                        throw; //Ném lỗi để lên catch ngoài, sẽ reconnect
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //App đang shutdown, không cần retry
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Listener disconnected, will retry...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
