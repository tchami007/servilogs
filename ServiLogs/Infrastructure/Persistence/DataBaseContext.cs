using Microsoft.EntityFrameworkCore;
using ServiLogs.Application.Models;

namespace ServiLogs.Infrastructure.Persistence
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<LogEntry> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogEntry>().HasKey(l => l.Guid);
        }
    }
}
