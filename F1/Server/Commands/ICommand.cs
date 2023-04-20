using F1.Presentation.Views.Menu;

namespace Server.Commands;

public interface ICommand
{
    CommandResult Execute(CommandQuery? query, Menu menu);
}