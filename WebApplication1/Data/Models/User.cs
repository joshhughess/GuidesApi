using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GuidesApi.Data.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>(); [NotMapped]
        public List<Claim> Claims => new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, Id),
            new Claim(ClaimTypes.Name, Email)
        }
        .Union(UserRoles.Select(x => x.Role.Name).Select(x => new Claim(ClaimTypes.Role, x)))
        .Union(UserRoles.Where(x => x.Role.Permissions.Any())
            .SelectMany(x => x.Role.Permissions.Select(p => p.Permission)).Select(x => new Claim(CustomClaimTypes.Permission, x))).ToList();
    }
}
