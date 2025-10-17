namespace RealtimeHub.API.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static void LoadConfiguration(this IConfiguration configuration, IWebHostEnvironment environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        if (environment.IsDevelopment())
        {
            builder.AddUserSecrets<Program>();
        }
    }
}
