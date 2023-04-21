using System.Text;

namespace ProtocolHelper.Communication.Models;

public class ProtocolData
{
    public const int OperationMaxLength = 2;
    public const int HeaderMaxLength = 3;
    public const int QueryMaxLength = 4;
    public string Header { get; }
    public string Operation { get; }
    public QueryData? Query { get; set; }
    public string QueryLength { get; }

    public ProtocolData(bool isReq, string operation, QueryData? query)
    {
        Header = isReq ? "REQ" : "RES";
        Operation = ConvertLength(operation, OperationMaxLength, 0, 99);
        Query = query;
        var serializedQuery = QueryDataSerializer.Serialize(Query);
        QueryLength = ConvertLength(
            $"{serializedQuery.Length}", 
            QueryMaxLength,
            0,
            9999
        );
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Header: {Header}");
        sb.AppendLine($"Operation: {Operation}");

        if (Query != null)
        {
            sb.AppendLine("Query:");
            foreach (var kvp in Query.Fields)
            {
                sb.AppendLine($"\t{kvp.Key}: {kvp.Value}");
            }            
        }

        return sb.ToString();
    }

    private string ConvertLength(string operation, int length, int min, int max)
    {
        var parseOp = int.TryParse(operation, out var res);
        if (parseOp)
        {
            if (res >= min && res <= max)
            {
                return PrependZeros(operation, length);
            }
        }
        throw new ArgumentException($"Value is not allowed ${operation}");
    }
    
    private string PrependZeros(string numberString, int allowedLength)
    {
        int length = numberString.Length;

        if (length < allowedLength)
        {
            int numZeros = allowedLength - length;
            string output = new string('0', numZeros) + numberString;

            return output;
        }

        return numberString;
    }

}