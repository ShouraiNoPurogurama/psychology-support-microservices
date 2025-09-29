using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace BuildingBlocks.Behaviors;

public static class SerilogExtensions
{
    /// <summary>
    /// Đăng ký Serilog vào HostBuilder với cấu hình tùy chỉnh cho microservices.
    /// </summary>
    public static IHostBuilder UseCustomSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceName = null,
        AnsiConsoleTheme? theme = null,
        LogEventLevel minimumLevel = LogEventLevel.Information
    )
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
   
        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            var loggerConfiguration = loggerConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

            //Thêm service name nếu được cung cấp
            if (!string.IsNullOrEmpty(serviceName))
            {
                loggerConfiguration.Enrich.WithProperty("ServiceName", serviceName);
            }

            //Thêm environment info
            loggerConfiguration.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            //Console sink (luôn enable, kể cả prod)
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}",
                theme: theme ?? AnsiConsoleTheme.Code
            );
            
        });
    }

    ///<summary>
    ///Extension method đơn giản cho các service không cần custom nhiều
    ///</summary>
    public static IHostBuilder UseStandardSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string serviceName)
    {
        return hostBuilder.UseCustomSerilog(
            configuration: configuration,
            serviceName: serviceName,
            theme: AnsiConsoleTheme.Code,
            minimumLevel: LogEventLevel.Information
        ); 
    }
}