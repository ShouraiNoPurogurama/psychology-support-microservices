using System.Security.Claims;

namespace Payment.API.Common
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

            // Chỉ cho Admin hoặc chính chủ sửa
            if (role == "Admin")
                return true;

            return patientProfileId == profileId;
        }
    }
}
