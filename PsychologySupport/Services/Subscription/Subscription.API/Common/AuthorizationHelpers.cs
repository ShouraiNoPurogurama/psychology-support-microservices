using System.Security.Claims;

namespace Subscription.API.Common
{
    public static class AuthorizationHelpers
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
        
        public static bool CanModifySystemData(ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);

            return role is "Admin" or "Manager";
        }

        public static bool HasViewAccessToPatientProfile(ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);

            return role is "Manager" or "Admin";
        }
        
        public static bool IsExclusiveAccess(ClaimsPrincipal user)
        {
            var id = user.FindFirstValue("userId");

            return id == "0197edba-e8e1-78ef-be6a-83e4110170e6";
        }
    }
}
