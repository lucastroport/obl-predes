using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class PartsCommand : ICommand
{
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        var cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"MENU", $"{MenuOptions.ListItems(MenuOptions.PartMenuItems)}"},
            }
        );
        return new CommandResult(cmdQuery);
    }
}