using System.Security.Claims;

namespace LifeStyles.API.Common
{
    public static class AuthorizationHelpers
    {
        public static bool HasAccessToPatientProfile(Guid patientProfileId, ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            var profileIdStr = user.FindFirstValue("profileId");

            if (!Guid.TryParse(profileIdStr, out var profileId))
                return false;

            if (role == "Admin")
                return true;

            return patientProfileId == profileId;
        }
    }
}
