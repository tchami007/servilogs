using ServiLogs.Application.Models;
using ServiLogs.Infrastructure.Persistence;

namespace ServiLogs.Application.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task RegisterLogAsync(LogEntry logEntry)
        {
            await _logRepository.SaveLogAsync(logEntry);
        }
    }
}
