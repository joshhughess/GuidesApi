namespace GuidesApi.Data.Models
{
    public class RolePermission : BaseEntity
    {
        public string RoleId { get; set; }
        public string Permission { get; set; }

        public virtual Role Role { get; set; }
    }
}
