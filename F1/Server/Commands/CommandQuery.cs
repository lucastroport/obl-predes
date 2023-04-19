namespace Server.Commands;

public class CommandQuery
{
    public Dictionary<string, string> Fields { get; set; }
    public CommandQuery(Dictionary<string, string> fields)
    {
        Fields = fields;
    }

    public void PopulateQuery(List<string> values)
    {
        Dictionary<string, string> resultDict = new Dictionary<string, string>();
        for (int i = 0; i < values.Count; i++)
        {
            string key = (i + 1).ToString();
            resultDict.Add(key, values[i]);
        }

        Fields = resultDict;
    }
}