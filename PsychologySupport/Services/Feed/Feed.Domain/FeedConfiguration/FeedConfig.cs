namespace Feed.Domain.FeedConfiguration;

public sealed class FeedConfig
{
    public string Key { get; }
    public string? Value { get; }

    private FeedConfig(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is required", nameof(key));

        Key = key.Trim();
        Value = value?.Trim();
    }

    public static FeedConfig Create(string key, string? value = null)
        => new(key, value);

    public FeedConfig UpdateValue(string? newValue)
        => new(Key, newValue);
}
