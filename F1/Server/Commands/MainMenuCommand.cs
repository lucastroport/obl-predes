using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class MainMenuCommand : ICommand
{
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername = null)
    {
        var cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"MENU", $"{MenuOptions.ListItems(MenuOptions.MenuItems)}"},
            }
        );
        return new CommandResult(cmdQuery);
    }
}