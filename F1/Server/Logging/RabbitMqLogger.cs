namespace Server.Logging;

using RabbitMQ.Client;
using System;
using System.Text;

public class RabbitMqLogger : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    public RabbitMqLogger(string host, string username, string password, string exchangeName)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _exchangeName = exchangeName;

        _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
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
        _channel.BasicPublish(exchange: _exchangeName, routingKey: type.ToString(), basicProperties: null, body: body);
    }


    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
