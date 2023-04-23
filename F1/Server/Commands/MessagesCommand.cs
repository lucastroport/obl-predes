using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class MessagesCommand : ICommand
{
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        var cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"MENU", $"{MenuOptions.ListItems(MenuOptions.MessagesMenuItems)}"},
            }
        );
        return new CommandResult(cmdQuery);
    }
}