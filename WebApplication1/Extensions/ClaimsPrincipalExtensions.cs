using System.Security.Claims;

namespace GuidesApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.Identity.Name;
        }

        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.HasClaim(CustomClaimTypes.SuperUser, $"{true}") || principal.HasClaim(CustomClaimTypes.Permission, permission);
        }

        /// <summary>
        /// Returns true if a user has any of the specified permissions
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Permissions"></param>
        /// <returns></returns>
        public static bool HasPermissions(this ClaimsPrincipal User, string[] Permissions)
        {
            return Permissions.Any(x => User.HasPermission(x));
        }
    }
}
