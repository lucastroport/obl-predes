using F1.Constants;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class LogOutCommand : ICommand
{
    private RabbitMqLogger rabbitMqLogger;
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMqLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        menu.TriggerNotAuthMenu();
        var cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "User logged out, bye."},
                {"MENU", $"{menu}"},
                {ConstantKeys.Logout, "true"}
            }
        );
        rabbitMqLogger.LogInfo($" (USER: {authUsername}) user {authUsername} logged out");
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}