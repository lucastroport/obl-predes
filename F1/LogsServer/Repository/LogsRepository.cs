using LogsServer.Models;

namespace LogsServer.Repository;

public class LogsRepository : ILogsRepository
{
    private readonly List<LogMessage> logMessages;

    public LogsRepository()
    {
        logMessages = new List<LogMessage>();
    }

    public void AddLog(LogMessage logMessage)
    {
        lock (logMessages)
        {
            logMessages.Add(logMessage);
        }
    }

    public List<LogMessage> GetLogs()
    {
        lock (logMessages)
        {
            return logMessages.ToList();
        }
    }
}