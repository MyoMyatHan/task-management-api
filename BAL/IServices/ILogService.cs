namespace BAL.IServices
{
    public interface ILogService
    {
        Task LogInfoAsync(string action, string message);
        Task LogWarningAsync(string action, string message);
        Task LogErrorAsync(string action, string message, string? stackTrace = null);
    }
}
