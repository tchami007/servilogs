using ServiLogs.Application.Models;

namespace ServiLogs.Application.Services
{
    public interface ILogService
    {
        Task RegisterLogAsync(LogEntry logEntry);
    }
}
