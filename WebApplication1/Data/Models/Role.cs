namespace GuidesApi.Data.Models
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public List<RolePermission> Permissions { get; set; }
        public List<UserRole> UserRoles { get; set; }
    }
}
