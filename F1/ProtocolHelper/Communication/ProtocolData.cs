using System.Text;

namespace ProtocolHelper.Communication;

public class ProtocolData
{
    public string Header { get; set; }
    public int Operation { get; set; }
    public QueryData Query { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Header: {Header}");
        sb.AppendLine($"Operation: {Operation}");

        sb.AppendLine("Query:");
        foreach (var kvp in Query.Fields)
        {
            sb.AppendLine($"\t{kvp.Key}: {kvp.Value}");
        }

        return sb.ToString();
    }
}