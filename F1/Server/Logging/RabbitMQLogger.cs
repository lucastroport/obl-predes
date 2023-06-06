namespace Server.Logging;

using RabbitMQ.Client;
using System;
using System.Text;

public class RabbitMQLogger : IDisposable
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string exchangeName;

    public RabbitMQLogger(string host, string username, string password, string exchangeName)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        this.exchangeName = exchangeName;

        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
    }
    
    public void LogInfo(string message)
    {
        LogMessage(message, MessageType.Info);
    }

    public void LogPartInfo(string message)
    {
        LogMessage(message, MessageType.Parts);
    }
    
    public void LogError(string message)
    {
        LogMessage(message, MessageType.ServerError);
    }
    
    public void LogFileInfo(string message)
    {
        LogMessage(message, MessageType.Files);
    }
    
    public void LogClientError(string message)
    {
        LogMessage(message, MessageType.ClientError);
    }

    private void LogMessage(string message, MessageType type)
    {
        var logMessage = $"{DateTime.Now:MM-dd-yyyy HH:mm} - {message}";
        var body = Encoding.UTF8.GetBytes(logMessage);
        channel.BasicPublish(exchange: exchangeName, routingKey: type.ToString(), basicProperties: null, body: body);
    }


    public void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
    }
}
