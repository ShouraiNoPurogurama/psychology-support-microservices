using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Wellness.API.Common
{
    public static class AuthorizationHelpers
    {
        // Xem thông tin của chính subject hoặc Admin/Manager
        public static bool CanView(Guid targetSubjectRef, ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            var currentSubjectStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(currentSubjectStr, out var currentSubjectRef))
                return false;

            if (targetSubjectRef == currentSubjectRef)
                return true;

            
            return role is "Manager" or "Admin";
        }

        // Chỉnh sửa thông tin của chính subject hoặc Admin
        public static bool CanModify(Guid targetSubjectRef, ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            var currentSubjectStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (role == "Admin")
                return true;

            if (!Guid.TryParse(currentSubjectStr, out var currentSubjectRef))
                return false;

            return targetSubjectRef == currentSubjectRef;
        }

        // Kiểm tra quyền xem chung dựa trên role
        public static bool HasViewAccess(ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role);
            return role is "User" or "Manager" or "Admin";
        }
    }
}
