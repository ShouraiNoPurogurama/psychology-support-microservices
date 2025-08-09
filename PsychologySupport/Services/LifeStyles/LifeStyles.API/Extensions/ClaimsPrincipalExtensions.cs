using System.Security.Claims;

namespace LifeStyles.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetProfileId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("profileId")?.Value;
        if (value == null)
            throw new InvalidOperationException("Access token không hợp lệ hoặc thiếu claim profileId.");

        if (!Guid.TryParse(value, out var profileId))
            throw new InvalidOperationException("Profile ID trong token không hợp lệ.");

        return profileId;
    }
}