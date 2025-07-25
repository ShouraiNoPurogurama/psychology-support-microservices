using System.Reflection;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration,
        Assembly? assembly = null) 
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.SetInMemorySagaRepositoryProvider();

            config.AddConsumers(assembly);
            config.AddSagaStateMachines(assembly);
            config.AddSagas(assembly);
            config.AddActivities(assembly);

            if (assembly is not null) config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"]!);
                    host.Password(configuration["MessageBroker:Password"]!);
                    host.Heartbeat(10); // default là 60s → giảm để phát hiện sớm
                    host.RequestedConnectionTimeout(TimeSpan.FromSeconds(30)); 
                });
                
                configurator.UseInMemoryOutbox(context); //giúp đảm bảo transactional message dispatch
                
                configurator.ConfigureEndpoints(context);
                
                configurator.UseMessageRetry(retryConfig =>
                {
                    retryConfig.Exponential(
                        3,              
                        TimeSpan.FromSeconds(1),    //thời gian delay tối thiểu
                        TimeSpan.FromSeconds(30),   //thời gian delay tối đa
                        TimeSpan.FromSeconds(5));  //hệ số tăng delay (exponential factor)
                    
                    retryConfig.Ignore<ValidationException>(); //lỗi không nên retry
                    retryConfig.Handle<TimeoutException>();     //lỗi nên retry
                });
                
                configurator.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);    
                    cb.TripThreshold = 40;    
                    cb.ActiveThreshold = 10;                       //Phải có ít nhất 10 request mới tính TripThreshold
                    cb.ResetInterval = TimeSpan.FromMinutes(5);    //Sau 5 phút thử đóng mạch lại
                });

            });
        });
        return services;
    }
}