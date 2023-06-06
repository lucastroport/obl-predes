namespace Server.Commands;

public class CommandQuery
{
    public Dictionary<string, string> Fields { get; }
    public CommandQuery(Dictionary<string, string> fields)
    {
        Fields = fields;
    }
}
