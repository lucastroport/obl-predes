namespace LogsServer.Models;

public class LogMessage
{
    public string Message { get; set; }
    public MessageType Category { get; set; }
}

public enum MessageType
{
    ServerError,
    Info,
    ClientError
}
