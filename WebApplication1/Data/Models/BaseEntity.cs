using System.ComponentModel.DataAnnotations.Schema;

namespace GuidesApi.Data.Models
{
    public abstract class BaseEntity : BaseEntity<string>
    {

    }
    public abstract class BaseEntity<TKey> where TKey : class
    {
        public TKey Id { get; set; }
        public string CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedById { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool ForceDelete { get; set; }
        [NotMapped]
        public bool OverrideAudit { get; set; }
    }
}
