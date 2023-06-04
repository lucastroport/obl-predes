using System.Text;
using LogsServer.Models;
using LogsServer.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogsServer;

public class MessageConsumer
{
    private readonly ILogsRepository _logRepository;

    public MessageConsumer(ILogsRepository logRepository)
    {
        _logRepository = logRepository;
    }
    
    public void StartConsuming()
    {
        const string host = "localhost";
        const string username = "guest";
        const string password = "guest";
        const string exchangeName = "logs";
        const string exchangeType = ExchangeType.Direct;

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchangeName, exchangeType, durable: true);
            var queueName = channel.QueueDeclare().QueueName;

            var queueBindings = new Dictionary<string, MessageType>
            {
                { "queue.serverError", MessageType.ServerError },
                { "queue.info", MessageType.Info },
                { "queue.clientError", MessageType.ClientError }
            };
            foreach (var queueBinding in queueBindings)
            {
                var logType = queueBinding.Value.ToString();
                channel.QueueBind(queueName, exchangeName, logType);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var routingKey = e.RoutingKey;
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received log message: " + message);
                var logMessage = new LogMessage
                {
                    Message = message,
                    Category = GetMessageCategory(routingKey)
                };
                lock (_logRepository)
                {
                    _logRepository.AddLog(logMessage);
                }

                // Acknowledge the message
                channel.BasicAck(e.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queueName, autoAck: false, consumer);
            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }

    private MessageType GetMessageCategory(string routingKey)
    {
        switch (routingKey)
        {
            case "queue.serverError":
                return MessageType.ServerError;
            case "queue.info":
                return MessageType.Info;
            case "queue.clientError":
                return MessageType.ClientError;
            default:
                return MessageType.Info; // Default category
        }
    }
}