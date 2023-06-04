using LogsServer.Models;

namespace LogsServer.Repository;

public interface ILogsRepository
{
    void AddLog(LogMessage logMessage);
    List<LogMessage> GetLogs();
}