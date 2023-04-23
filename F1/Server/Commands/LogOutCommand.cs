using F1.Constants;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LogOutCommand : ICommand
{
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        menu.TriggerNotAuthMenu();
        var cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "User logged out, bye."},
                {"MENU", $"{menu}"},
                {ConstantKeys.Logout, "true"}
            }
        );
        return new CommandResult(cmdQuery);
    }
}