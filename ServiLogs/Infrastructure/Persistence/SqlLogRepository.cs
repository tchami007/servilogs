using ServiLogs.Application.Models;

namespace ServiLogs.Infrastructure.Persistence
{
    public class SqlLogRepository : ILogRepository
    {
        private readonly DatabaseContext _context;

        public SqlLogRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task SaveLogAsync(LogEntry logEntry)
        {
            await _context.Logs.AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
