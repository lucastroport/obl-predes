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

    public void Log(string message, MessageType type = MessageType.Info)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: exchangeName, routingKey: type.ToString(), basicProperties: null, body: body);
    }

    public void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
    }
}
