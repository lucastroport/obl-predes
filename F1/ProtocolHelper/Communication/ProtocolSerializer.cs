using System.Text;

namespace ProtocolHelper.Communication;

public static class ProtocolSerializer
{
    public static string Serialize(ProtocolData data)
    {
        string queryStr = data.Query != null ? QueryDataSerializer.Serialize(data.Query) : "";
        return $"header={data.Header}:op={data.Operation}:{queryStr}";
    }

    public static ProtocolData Deserialize(string data)
    {
        ProtocolData protocolData = new ProtocolData();
        string[] fields = data.Split(':');
        foreach (string field in fields)
        {
            string[] keyValue = field.Split('=');
            switch (keyValue[0])
            {
                case "header":
                    protocolData.Header = keyValue[1];
                    break;
                case "op":
                    protocolData.Operation = int.Parse(keyValue[1]);
                    break;
                default:
                    protocolData.Query = QueryDataSerializer.Deserialize(RemoveFirstOccurrence(field,"query="));
                    break;
            }
        }
        return protocolData;
    }
    
    private static string RemoveFirstOccurrence(string inputString, string substring)
    {
        var index = inputString.IndexOf(substring, StringComparison.Ordinal);
        return index < 0 ? inputString :
            inputString.Remove(index, substring.Length);
    }
}