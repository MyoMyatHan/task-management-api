using BAL.IServices;
using MODEL;
using MODEL.Entities;

namespace BAL.Services
{
    public class LogService : ILogService
    {
        private readonly DataContext _context;

        public LogService(DataContext context)
        {
            _context = context;
        }

        public async Task LogInfoAsync(string action, string message)
        {
            await WriteLog("Info", action, message, null);
        }

        public async Task LogWarningAsync(string action, string message)
        {
            await WriteLog("Warning", action, message, null);
        }

        public async Task LogErrorAsync(string action, string message, string? stackTrace = null)
        {
            await WriteLog("Error", action, message, stackTrace);
        }

        private async Task WriteLog(string level, string action, string message, string? stackTrace)
        {
            try
            {
                var log = new SystemLog
                {
                    LogId = Guid.NewGuid(),
                    Level = level,
                    Action = action,
                    Message = message,
                    StackTrace = stackTrace,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.SystemLogs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Logging must never throw — silently swallow errors
            }
        }
    }
}
