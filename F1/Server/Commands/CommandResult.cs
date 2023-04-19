namespace Server.Commands;

public class CommandResult
{
    public CommandQuery CommandQuery { get; set; }

    public CommandResult(CommandQuery query)
    {
        CommandQuery = query;
    }
}