using System.Security.Claims;

namespace Test.API.Common
{
    public class AuthorizationHelpers
    {
        public static bool CanViewPatientProfile(Guid patientProfileId, ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            var profileIdStr = user.FindFirstValue("profileId");

            if (!Guid.TryParse(profileIdStr, out var profileId))
                return false;

            if (patientProfileId == profileId)
                return true;

            return role is "Manager" or "Admin";
        }

        public static bool CanModifyPatientProfile(Guid patientProfileId, ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            var profileIdStr = user.FindFirstValue("profileId");

            if (!Guid.TryParse(profileIdStr, out var profileId))
                return false;

            if (role == "Admin")
                return true;

            return patientProfileId == profileId;
        }

        public static bool HasViewAccessToPatientProfile(ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);

            return role is "User" or "Manager" or "Admin";
        }
    }
}
