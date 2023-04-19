namespace Server.Commands;

public interface ICommand
{
    CommandResult Execute(CommandQuery? query);
}