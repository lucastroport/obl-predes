using System.Reflection.Metadata;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class AddUserCommand : ICommand
{
    private IUserRepository _userRepository;
    private RabbitMqLogger rabbitMqLogger;
    private static readonly object UserAddLock = new();
    private static readonly object UserQueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMqLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.UsernameKey, "Enter"},
                {ConstantKeys.PasswordKey, "Enter"}
            });
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }
        _userRepository = UserRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.UsernameKey, out var username);
        query.Fields.TryGetValue(ConstantKeys.PasswordKey, out var password);

        User? authenticatedUser;
        bool userExists;
        
        lock (UserQueryLock)
        {
            authenticatedUser = _userRepository.QueryByUsername(authUsername);
            userExists = _userRepository.QueryByUsername(username) != null;   
        }

        if (authenticatedUser != null && authenticatedUser.Type == UserType.Admin)
        {
            if (userExists)
            {
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "ERROR: User already exists."},
                        {"MENU", $"{menu}"}
                    }
                );
                rabbitMqLogger.LogClientError($" (USER: {authUsername}) User {username} already exists");
            }
            else
            {
                lock (UserAddLock)
                {
                    _userRepository.AddUser(
                        new User(username, password)
                    );   
                }

                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "User registered correctly."},
                        {"MENU", $"{menu}"}
                    }
                );
                rabbitMqLogger.LogInfo($" (USER: {authUsername}) User {username} registered correctly");
            }
        }
        else
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "ERROR: You need admin access to add a user."},
                    {"MENU", $"{menu}"}
                }
            );
            rabbitMqLogger.LogClientError($" (USER: {authUsername}) You need admin access to add a user.");
        }
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}