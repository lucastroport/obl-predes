using F1.Constants;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LoginCommand : ICommand
{
    public CommandResult Execute(CommandQuery? query)
    {
        if (query == null)
        {
            var cmdQuery = new CommandQuery(new Dictionary<string, string>());
            cmdQuery.PopulateQuery(new List<string>{"user","password"});
            
            return new CommandResult(cmdQuery);
        }
        //TODO Grab credentials an log in
        return new CommandResult(
            new CommandQuery(
                new Dictionary<string, string>
                {
                    { "MENU", $"{MenuItemConstants.MainMenu}" }
                }
            )
            );
    }
}