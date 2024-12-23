using ServiLogs.Application.Models;

namespace ServiLogs.Infrastructure.Persistence
{
    public interface ILogRepository
    {
        Task SaveLogAsync(LogEntry logEntry);
    }
}
