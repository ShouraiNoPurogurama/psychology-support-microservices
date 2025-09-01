using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.FileProviders;
using Profile.API.Extensions;

namespace Profile.API.Data;

public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    protected abstract string MigrationConnKey { get; }
    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    public TContext CreateDbContext(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                      ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? "Production";

        var env = new HostingEnvironment { EnvironmentName = envName };

        var config = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .LoadConfiguration(env)
            .Build();

        var envOverride = Environment.GetEnvironmentVariable(MigrationConnKey.Replace(':', '_').ToUpperInvariant());

        var connStr = envOverride ?? config.GetConnectionString(MigrationConnKey);
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException("Could not find a connection string in the application settings.");
        }

        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connStr)
            .Options;

        return CreateNewInstance(options);
    }

    private class HostingEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}