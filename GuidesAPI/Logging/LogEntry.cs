namespace GuidesApi.Logging
{
    public class LogEntry
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string User { get; set; }
        public string UserId { get; set; }
    }
}
