using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class SendMessageCommand : ICommand
{
    private RabbitMQLogger rabbitMqLogger;
    private IMessageRepository _messageRepository;
    private IUserRepository _userRepository;
    private static readonly object QueryByUsernameLock = new();
    private static readonly object QueryUsersLock = new();
    private static readonly object QueryByIdLock = new();
    private static readonly object AddMessageLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMQLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        CommandQuery? cmdQuery;
        _userRepository = UserRepository.Instance;
        _messageRepository = MessageRepository.Instance;

        User? authenticatedUser;
        lock (QueryByUsernameLock)
        {
            authenticatedUser = _userRepository.QueryByUsername(authUsername);   
        }

        if (query == null)
        {
            List<User> users = new List<User>();
            lock (QueryUsersLock)
            {
                users = _userRepository.GetAllMechanicUsers().FindAll(u => !u.Username.Equals(authUsername));
            }
            
            if (authenticatedUser is { Type: UserType.Admin })
            {
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "ERROR: Messaging is only between mechanics"},
                        {"MENU", $"{menu}"}
                    }
                );
                rabbitMqLogger.LogClientError($" (USER: {authUsername}) Messaging is only between mechanics");
            }
            else
            {
                if (users.Count == 0)
                {
                    cmdQuery  = new CommandQuery(
                        new Dictionary<string, string>
                        {
                            {"RESULT", "There are no users to send a message to"},
                            {"MENU", $"{menu}"}
                        }
                    );
                    rabbitMqLogger.LogInfo($" (USER: {authUsername}) There are no users to send a message to");
                }
                else
                {
                    cmdQuery = new CommandQuery(new Dictionary<string, string>
                    {
                        {ConstantKeys.SelectUserKey, $"{users.Aggregate("", (menuString, item) => menuString + item + "\n")}"},
                        {ConstantKeys.SendMessageKey, "Enter"}
                
                    });
                }   
            }
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }
        
        query.Fields.TryGetValue(ConstantKeys.SelectUserKey, out var userId);
        query.Fields.TryGetValue(ConstantKeys.SendMessageKey, out var message);

        User? userToSend;
        lock (QueryByIdLock)
        {
            userToSend = _userRepository.QueryUserById(userId);   
        }

        if (userToSend is { Type: UserType.Mechanic })
        {
            lock (AddMessageLock)
            {
                var messageToAdd = new Message(message, authenticatedUser.Id, userId );
                _messageRepository.AddMessage(messageToAdd);
            }
            rabbitMqLogger.LogInfo($" (USER: {authUsername}) Message sent to user with id {userId}");
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "Message sent !"},
                    {"MENU", $"{menu}"}
                }
            );
        }
        else
        {
            rabbitMqLogger.LogClientError($" (USER: {authUsername}) User entered not valid.");
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "ERROR: User entered not valid."},
                    {"MENU", $"{menu}"}
                }
            );
        }
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}