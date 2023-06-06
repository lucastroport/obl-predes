using System.Runtime.InteropServices;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class LoginCommand : ICommand
{
    private RabbitMqLogger rabbitMqLogger;
    private IUserRepository _userRepository;
    private static readonly object QueryByUsernameLock = new();
    private static readonly object AuthAddUsernameLock = new();
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

            return new CommandResult(cmdQuery);
        }

        _userRepository = UserRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.UsernameKey, out var username);
        query.Fields.TryGetValue(ConstantKeys.PasswordKey, out var password);

        User? foundUser;

        lock (QueryByUsernameLock)
        {
            foundUser = _userRepository.QueryByUsername(username);    
        }
        
        string resultMessage = "User or password incorrect";
        
        if (foundUser != null)
        {
            if (password.Equals(foundUser.Password))
            {
                if (!foundUser.IsLoggedIn)
                {
                    foundUser.IsLoggedIn = true;
                    resultMessage = "Login successful";
                    menu.TriggerLoggedInMenu();
                    rabbitMqLogger.LogInfo($" (USER: {foundUser.Username}) Login successful");
                }
                else
                {
                    cmdQuery = new CommandQuery(
                        new Dictionary<string, string>
                        {
                            {"RESULT", "You user is currently logged in another device, please log out on the device before logging in here."},
                            {"MENU", $"{menu}"}
                        }
                    );
                    rabbitMqLogger.LogClientError($" (USER: {authUsername}) You user is currently logged in another device, please log out on the device before logging in here.");
                    rabbitMqLogger.Dispose();
                    return new CommandResult(cmdQuery);
                }
            }
        }
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", $"{resultMessage}"},
                {"MENU", $"{menu}"}
            }
            );
        if (foundUser != null)
        {
            lock (AuthAddUsernameLock)
            {
                cmdQuery.Fields.Add("AUTHENTICATED", foundUser.Username);   
            }
        }
        else
        {
            rabbitMqLogger.LogClientError($" (USER: {authUsername}) User or password incorrect");
        }
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}