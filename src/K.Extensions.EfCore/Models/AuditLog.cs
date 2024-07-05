namespace K.Extensions.EfCore.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; }
        public string Changes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
