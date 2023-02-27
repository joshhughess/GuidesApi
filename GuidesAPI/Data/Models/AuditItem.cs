using System.ComponentModel.DataAnnotations;

namespace GuidesApi.Data.Models
{
    public class AuditItem
    {
        [Key]
        public int AuditId { get; set; }
        public string RequestId { get; set; }
        public string Table { get; set; }
        public string PK { get; set; }
        public string Environment { get; set; }
        public int Duration { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
        public string EntityType { get; set; }
        public string UserId { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
    }
}
