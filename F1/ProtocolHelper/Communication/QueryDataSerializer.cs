namespace ProtocolHelper.Communication;

public class QueryDataSerializer
{
    public static string Serialize(QueryData data)
    {
        return string.Join("&", data.Fields.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    public static QueryData Deserialize(string data)
    {
        QueryData queryData = new QueryData();
        string[] fields = data.Split('&');
        foreach (string field in fields)
        {
            string[] keyValue = field.Split('=');
            if (keyValue.Length == 2)
            {
                queryData.Fields.Add(keyValue[0], keyValue[1]);
            }
        }
        return queryData;
    }
}