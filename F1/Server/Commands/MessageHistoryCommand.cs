using F1.Domain.Comparator;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class MessageHistoryCommand : ICommand
{
    private RabbitMQLogger rabbitMqLogger;
    private IMessageRepository _messageRepository;
    private IUserRepository _userRepository;
    private static readonly object QueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername = null)
    {
        rabbitMqLogger = new RabbitMQLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        CommandQuery? cmdQuery;
        _messageRepository = MessageRepository.Instance;
        _userRepository = UserRepository.Instance;

        User? receiverUser;
        List<Message> messages = new List<Message>();
        lock (QueryLock)
        {
            receiverUser = _userRepository.QueryByUsername(authUsername);
            messages = _messageRepository.GetAllMessagesForUser(receiverUser.Id);   
        }

        messages.Sort(new MessageDateComparer());
        
        if (messages.Count > 0)
        {
            var history = $"\n{messages.Aggregate("", (menuString, item) => menuString + item + "\n")}";
                
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", $"{history}"},
                    {"MENU", $"{menu}"}
                }
            );
            rabbitMqLogger.LogInfo($" (USER: {authUsername}) Messages retrieved");
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
            
        }
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "You don't have any messages"},
                {"MENU", $"{menu}"},
            }
        );
        rabbitMqLogger.LogInfo($" (USER: {authUsername}) You don't have any messages");
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}