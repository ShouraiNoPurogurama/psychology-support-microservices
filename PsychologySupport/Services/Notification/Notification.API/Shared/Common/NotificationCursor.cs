using System.Text;

namespace Notification.API.Shared.Common;

public static class NotificationCursor
{
    public static string Encode(DateTimeOffset createdAt, Guid id)
    {
        var cursorString = $"{createdAt.UtcTicks}:{id}";
        var bytes = Encoding.UTF8.GetBytes(cursorString);
        return Convert.ToBase64String(bytes);
    }

    public static (DateTimeOffset CreatedAt, Guid Id)? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var cursorString = Encoding.UTF8.GetString(bytes);
            var parts = cursorString.Split(':');

            if (parts.Length != 2)
            {
                return null;
            }

            if (!long.TryParse(parts[0], out var ticks) ||
                !Guid.TryParse(parts[1], out var id))
            {
                return null;
            }

            return (new DateTimeOffset(ticks, TimeSpan.Zero), id);
        }
        catch
        {
            return null;
        }
    }
}
