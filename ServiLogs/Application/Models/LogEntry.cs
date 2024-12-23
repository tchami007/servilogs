namespace ServiLogs.Application.Models
{
    public class LogEntry
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public string ApplicationName { get; set; }
        public string AdditionalData { get; set; }
        public Guid Guid { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
