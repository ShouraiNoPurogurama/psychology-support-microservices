namespace Notification.API;

public class AppSettings
{
    public ServiceDbContextConfiguraion ServiceDbContext { get; set; } = null!;
    public BrokerConfiguration BrokerConfiguration { get; set; } = null!;
    public FeatureConfiguration Features { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}

public class ServiceDbContextConfiguraion
{
    public required string NotificationDb { get; set; }
}

public class BrokerConfiguration
{
    public required string Host { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}


/// <summary>
/// At compile time, the C# compiler automatically combines all partial class definitions into a single class.
/// </summary>
public partial class FeatureConfiguration
{

}
